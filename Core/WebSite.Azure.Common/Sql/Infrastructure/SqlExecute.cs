using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NLog;
using Website.Azure.Common.Properties;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Execution;
using Website.Infrastructure.Sharding;
using Website.Infrastructure.Types;

namespace Website.Azure.Common.Sql.Infrastructure
{
    public class CountResult
    {
        public int Count { get; set; }
    }

    public class FederationInfo
    {
        public int FederationId { get; set; }
        public string Name { get; set; }
        public string DistributionName { get; set; }
        public int MemberId { get; set; }
        public string RangeLow { get; set; }
        public string RangeHigh { get; set; }
        public string FedTyp { get; set; }
        public bool IsRangeFor(IComparable val)
        {
            var low = SerializeUtil.ConvertVal(RangeLow, val.GetType()) as IComparable;
            if (low == null || !val.IsGreaterThanOrEqualTo(low))
                return false;

            if (string.IsNullOrEmpty(RangeHigh))
                return true;

            var high = SerializeUtil.ConvertVal(RangeHigh, val.GetType()) as IComparable;
            return high != null && (val.IsLessThanOrEqualTo(high));
        }
    }


    public static class SqlExtensions
    {
        public static string ToEqualExpression(this object val)
        {
            if (val != null && !val.IsNumeric())
                return '\'' + val.ToString() + '\'';
            return val != null ? val.ToString() : null;
        }

        public static SqlXml ToSql(this XElement element)
        {
            using (var streamreader = new StringReader(element.ToString()))
            {
                using (var xmlReader = XmlReader.Create(streamreader))
                {
                    return new SqlXml(xmlReader);
                }
            }
        }
    }

    public class AdoExecutionContext : IDisposable
    {
        public IDbConnection Connection { get; set; }
        public IDbTransaction Transaction { get; set; }

        public static string IdFor(ExecutionEnvironment env)
        {
            return "AdoExecutionContext" + env.Id;
        }

        public void Dispose()
        {
            if (Transaction != null)
                Transaction.Dispose();

            if (Connection != null)
                Connection.Dispose();
        }
    }

    public static class AdoActionExtensions
    {

        public static void ExecuteInEnvironment(this Action<IDbConnection> action, ExecutionEnvironment env, Func<IDbConnection> defaultFactory = null)
        {
            Action sqlAct = () =>
            {
                using (var conn = GetConnectionForEnvironment(env, defaultFactory))
                {
                    action(conn);
                }

            };

            sqlAct.ExecuteSqlWithRetries();
        }

        public static void ExecuteInContext(this Action<AdoExecutionContext> action, AdoExecutionContext context)
        {
            Action sqlAct = () => action(context);
            sqlAct.ExecuteSqlWithRetries();
        }

        public static TRet ExecuteInEnvironment<TRet>(this Func<IDbConnection, TRet> action, ExecutionEnvironment env, Func<IDbConnection> defaultFactory = null)
        {
            var ret = default(TRet);
            Action sqlAct = () =>
            {
                using (var conn = GetConnectionForEnvironment(env, defaultFactory))
                {
                    ret = action(conn);
                }

            };

            sqlAct.ExecuteSqlWithRetries();
            return ret;
        }

        public static IDbConnection GetConnectionForEnvironment(this ExecutionEnvironment env, Func<IDbConnection> defaultFactory = null)
        {
            if (env == null && defaultFactory == null) return null;

            var connection = env == null ? defaultFactory() : new SqlConnection(env.ConnectionInfo);
            if (connection == null) return null;
            connection.Open();
            return connection;
        }
    }

    public static class SqlExecute
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(SqlExecute).Name);

        public static bool ExecuteSqlWithRetries(this Action tryact)
        {
            return ExecuteSqlActionWithRetries(tryact);
        }

        public static bool ExecuteSqlActionWithRetries(Action tryact)
        {
            var retry = 0;
            do
            {
                try
                {
                    tryact();
                    retry = 0;
                }
                catch (SqlException ex)
                {
                    retry = HandleSqlException(ex, retry);
                    
                }

            } while (retry > 0);


            return retry == 0;
        }

        private static readonly HashSet<int> RetryErrors = new HashSet<int>()
                                                                         {
                                                                            10054,// Connection reset by peer
                                                                            10053, // Software caused connection abort
                                                                            1205, // Deadlock
                                                                         };
        private const int SqlRetryMax = 3;
        private const int RetryDelay = 10;
        private static int HandleSqlException(SqlException ex, int retryNumber = 0)
        {

            if (RetryErrors.Contains(ex.Number) && retryNumber < SqlRetryMax)
            {
                Thread.Sleep(RetryDelay);
                return retryNumber + 1;
            }
                

            var errorMessages = new StringBuilder("SQL Error ");
            if (retryNumber == SqlRetryMax)
                errorMessages.Append("giving up after " + SqlRetryMax + " retries");
            errorMessages.Append("\n");

            for (int i = 0; i < ex.Errors.Count; i++)
            {
                errorMessages.Append("Index #" + i + "\n" +
                    "Message: " + ex.Errors[i].Message + "\n" +
                    "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                    "Source: " + ex.Errors[i].Source + "\n" +
                    "Procedure: " + ex.Errors[i].Procedure + "\n");
            }

            errorMessages.Append("Local Stack: \n");
            var trace = new StackTrace();
            errorMessages.Append(trace.ToString());

            Logger.ErrorException(errorMessages.ToString(), ex);
            throw ex;
        }


        #region todo convert

        public const int Srid = 4326;
        public const int CommandTimeout = 120;


        public static string GetConnectionStringFromConfig(string settingName, string dbName = null)
        {
            var connectionString = Config.Instance.GetSetting(settingName);

            if (String.IsNullOrWhiteSpace(dbName))
                return connectionString;


            var csBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = dbName };
            return csBuilder.ToString();
        }

        //note must be applied within the context of an open connection
        private static void CheckUseFederationFor<FedType>(FedType source, SqlConnection connection)
        {
            var fedInstance = source.GetFedInstance();
            if (fedInstance == null)
                return;

            UseFederationFor(connection, fedInstance);
        }


        //note must be applied within the context of an open connection
        private static void UseFederationFor(SqlConnection connection, FederationInstance federationInstance)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = CommandTimeout;

            command.CommandText = String.Format(
                Resources.DbUseFederation,
                federationInstance.FederationName, federationInstance.DistributionName, federationInstance.FedVal.ToEqualExpression());

            command.ExecuteNonQuery();
        }

        private static void UseFederationRoot(SqlConnection connection)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = CommandTimeout;
            command.CommandText = Resources.DbUseFederationRoot;
            command.ExecuteNonQuery();
        }

        public static bool ExecuteCommand(string sqlCmd, SqlConnection connection, object parameters = null, bool isStoredProc = false)
        {
            Action tryact =
                () =>
                {
                    using (var conn = new SqlOpenClose(connection))
                    {
                        var command = conn.Cmd;
                        command.CommandText = sqlCmd;

                        if (isStoredProc)
                            command.CommandType = CommandType.StoredProcedure;

                        //                            if (parameters != null)
                        //                                AddParameters(command.Parameters, parameters);

                        //                            ExecuteCommand(command);
                    }
                };

            //            return ExecuteSqlActionWithRetries(tryact);
            return false;
        }

        public static bool ExecuteCommandInRecordContext(object recordContext, string sqlCmd, SqlConnection connection, object parameters = null, bool isStoredProc = false)
        {

            Action tryact =
                () =>
                {
                    using (var conn = new SqlOpenClose(connection))
                    {
                        CheckUseFederationFor(recordContext, connection);

                        var command = conn.Cmd;
                        command.CommandText = sqlCmd;

                        if (isStoredProc)
                            command.CommandType = CommandType.StoredProcedure;

                        //                            if (parameters != null)
                        //                                AddParameters(command.Parameters, parameters);

                        //                            ExecuteCommand(command);
                    }
                };

            //            return ExecuteSqlActionWithRetries(tryact);
            return false;
        }

        public static IEnumerable<RecordType> Query<RecordType>(string command, SqlConnection connection, FederationInstance federationInstance, object parameters, bool isStoredProc = false) where RecordType : new()
        {
            IEnumerable<RecordType> ret = new List<RecordType>();
            Action tryact =
                () =>
                {
                    using (var conn = new SqlOpenClose(connection))
                    {
                        if (federationInstance != null)
                            UseFederationFor(connection, federationInstance);

                        conn.Cmd.CommandText = command;

                        if (isStoredProc)
                            conn.Cmd.CommandType = CommandType.StoredProcedure;

                        //                            if (parameters != null)
                        //                                AddParameters(conn.Cmd.Parameters, parameters);

                        //                            ret = Query<RecordType>(conn.Cmd);
                    }
                };

            //            ExecuteSqlActionWithRetries(tryact);
            return ret;
        }

        private static readonly ConcurrentDictionary<string, List<FederationInfo>> FedInfoCache = new ConcurrentDictionary<string, List<FederationInfo>>();
        public static IEnumerable<FederationInfo> GetFederationInfo(SqlConnection connection, bool useCache = true)
        {
            Func<string, List<FederationInfo>> func = s => Query<FederationInfo>(Resources.DbGetFederationInfo, connection).ToList();
            return useCache ? FedInfoCache.GetOrAdd(connection.ConnectionString, func) : func(connection.ConnectionString);
        }

        public static IEnumerable<FederationInstance> GetFederationInstances(SqlConnection connection)
        {
            return GetFederationInfo(connection)
                .Select(fi => new FederationInstance()
                {
                    FederationName = fi.Name,
                    DistributionName = fi.DistributionName,
                    FedVal = fi.RangeLow
                });
        }

        //performs operation in all federations if a federated type
        public static bool ExecuteCommandInRecordTypeContext(Type contextRecordTyp, string sqlCmd,
            SqlConnection connection, object parameters = null, bool isStoredProc = false)
        {
            var fedInfo = contextRecordTyp.GetFedInfo();
            if (fedInfo == null)
                return ExecuteCommand(sqlCmd, connection, parameters);

            var federations = GetFederationInstances(connection)
                    .Where(fi => fi.FederationName.Equals(fedInfo.FederationName))
                    .ToList();
            if (!federations.Any())
                return ExecuteCommand(sqlCmd, connection, parameters);


            var connectionFact = new SqlConnectionFactory(connection);
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Min(8, federations.Count()) };
            var res = Parallel.ForEach(federations
                , parallelOptions
                , (fedShard, loopstate)
                    =>
                {
                    using (var shardconn = connectionFact.GetConnection())
                    {
                        var fedOb = Activator.CreateInstance(contextRecordTyp, true);
                        fedOb.SetFedVal(fedShard.FedVal);
                        if (!ExecuteCommandInRecordContext(fedOb, sqlCmd, shardconn, parameters, isStoredProc))
                            loopstate.Stop();

                    }
                });

            return res.IsCompleted;
        }

        private static IList<FederationInstance> GetFederationRangesFor(FederationInstance sourceFederationInfo
            , SqlConnection connection, IEnumerable<object> federationValues = null)
        {
            var values = federationValues as IList<object> ??
                (federationValues == null ? new List<object>() : federationValues.ToList());

            var info = GetFederationInfo(connection).ToList();
            var infoInRange = info.Where(
                fi => fi.Name.Equals(sourceFederationInfo.FederationName)
                    && (!values.Any() || values.Any(o => fi.IsRangeFor(o as IComparable)))).ToList();

            var ret = infoInRange.Any() ? infoInRange : info;

            return ret.Select(fi => new FederationInstance()
            {
                FederationName = fi.Name,
                DistributionName = fi.DistributionName,
                FedVal = fi.RangeLow
            }).ToList();

        }

        public static IEnumerable<RecordType> Query<RecordType>
            (string command, SqlConnection connection, object[] federationValues = null, object parameters = null, bool isStoredProc = false)
            where RecordType : new()
        {
            //if it isn't a federated type just perform normal queryParams
            var contextRecordTyp = typeof(RecordType);
            var fedInfo = contextRecordTyp.GetFedInfo();
            if (fedInfo == null)
            {
#if DEBUG
                if (federationValues != null && federationValues.Length > 0)
                {
                    if (federationValues.Any(f => (f as IComparable) == null))
                        throw new ArgumentException("this probably means your shit will break under federations");
                }
#endif
                return Query<RecordType>(command, connection, (FederationInstance)null, parameters, isStoredProc);
            }


            var federations = GetFederationRangesFor(fedInfo, connection, federationValues);
            var connectionFact = new SqlConnectionFactory(connection);
            var ret = new ConcurrentQueue<RecordType>();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Min(8, federations.Count()) };
            var res = Parallel.ForEach(federations
                , parallelOptions
                , (fedShard)
                     =>
                {
                    using (var taskconnection = connectionFact.GetConnection())
                    {
                        foreach (var record in Query<RecordType>(command, taskconnection, fedShard, parameters, isStoredProc))
                            ret.Enqueue(record);
                    }
                });

            return ret;
        }


        //        private static object ConvertResult(SqlDataReader reader, int ordinal, Type needed)
        //        {
        //            var result = reader[ordinal];
        //            if (result == null || result is System.DBNull)
        //                return null;
        //
        //            if (needed.GetNullTypeOrDefault() == typeof(SqlXml))
        //                return reader.GetSqlXml(ordinal);
        //
        //            //fixed with assembly binding
        ////            if (needed == typeof(SqlGeometry))
        ////                return SqlGeometry.Deserialize(reader.GetSqlBytes(ordinal));
        //
        //            return SerializeUtil.ConvertVal(result, needed);
        //        }


        #endregion
    }
}
