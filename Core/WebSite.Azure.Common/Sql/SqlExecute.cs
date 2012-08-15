using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SqlServer.Types;
using Microsoft.WindowsAzure.ServiceRuntime;
using WebSite.Azure.Common.Properties;
using WebSite.Infrastructure.Util;

namespace WebSite.Azure.Common.Sql
{
    public static class SqlExtensions
    {
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

        public static FederationInstance GetFedInstance(this object fedobject)
        {
            var prop = SerializeUtil.GetPropertyWithAttribute(fedobject.GetType(), typeof(FederationCol));
            if (prop == null || SqlExecute.FederationDisabled)
                return null;

            var fedAtt = prop.GetCustomAttributes(true).First(a => a.GetType() == typeof(FederationCol)) as FederationCol;

            var fedVal = prop.GetValue(fedobject, null);

            return new FederationInstance()
                       {
                           FederationName = fedAtt.FederationName,
                           DistributionName = fedAtt.DistributionName,
                           FedVal = fedVal
                       };
        }

        public static bool SetFedVal(this object fedObject, object fedVal)
        {
            var prop = SerializeUtil.GetPropertyWithAttribute(fedObject.GetType(), typeof(FederationCol));
            if (prop == null || SqlExecute.FederationDisabled)
                return false;

            prop.SetValue(fedObject, SqlExecute.ConvertVal(fedVal, prop.PropertyType), null);
            return true;
        }

        public static FederationInstance GetFedInfo(this Type fedTyp)
        {
            var prop = SerializeUtil.GetPropertyWithAttribute(fedTyp, typeof(FederationCol));
            if (prop == null || SqlExecute.FederationDisabled)
                return null;

            var fedAtt = prop.GetCustomAttributes(true).First(a => a.GetType() == typeof(FederationCol)) as FederationCol;

            return new FederationInstance()
            {
                FederationName = fedAtt.FederationName,
                DistributionName = fedAtt.DistributionName,
                FedVal = null
            };
        }
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
    }

    public class FederationInstance
    {
        public override string ToString()
        {
            return FederationName + " " + DistributionName + "=" + FedVal;
        }
        public string FederationName { get; set; }
        public string DistributionName { get; set; }
        public object FedVal { get; set; }
    }

    public class CountResult
    {
        public int Count { get; set; }
    }

    public class SqlExecute
    {
        public const int Srid = 4326;

        public static readonly Dictionary<Type, string> TypeToDbTypeDictionary
            = new Dictionary<Type, string>()
                  {
                      {typeof(string), "nvarchar(MAX)"},
                      {typeof(Guid), "uniqueidentifier"},
                      {typeof(int), "int"},
                      {typeof(double), "float"},
                      {typeof(DateTimeOffset), "datetimeoffset(7)"},
                      {typeof(SqlXml), "xml"},
                      {typeof(SqlGeography), "geography"},  
                      {typeof(long), "bigint"},                                            
                  };

        public static readonly Dictionary<Type, string> TypeToUdtTypeDictionary
            = new Dictionary<Type, string>()
                  {
                      //{typeof(SqlXml), "xml"},
                      {typeof(SqlGeography), "geography"},                                                                  
                  };

        private static bool? _federationDisabled;
        public static bool FederationDisabled
        {
            get
            {
                if (_federationDisabled.HasValue)
                    return _federationDisabled.Value;

                bool ret;
                bool.TryParse(
                (RoleEnvironment.IsAvailable)
                                       ? RoleEnvironment.GetConfigurationSettingValue("DisableFederation")
                                       : ConfigurationManager.AppSettings["DisableFederation"],
                
                out ret);
                _federationDisabled = ret;
                return ret;
            }
        }
        public static string GetConnectionStringFromConfig(string settingName, string dbName = null)
        {
            var connectionString = (RoleEnvironment.IsAvailable)
                                       ? RoleEnvironment.GetConfigurationSettingValue(settingName)
                                       : ConfigurationManager.AppSettings[settingName];

            if (String.IsNullOrWhiteSpace(dbName))
                return connectionString;


            var csBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = dbName };
            return csBuilder.ToString();
        }

        public static bool InsertOrUpdate<RecordType>(RecordType insert, SqlConnection connection, string tableName = null)
        {
            Type recordTyp = typeof (RecordType);
            if(String.IsNullOrWhiteSpace(tableName))
                tableName = recordTyp.Name;
            
            var keyCol = GetPrimaryKey(recordTyp);
            if(keyCol == null)
                throw new ArgumentNullException(String.Format("no key column for type {0}", recordTyp.Name));


            var vals = new Dictionary<string, object>();
            SerializeUtil.PropertiesToDictionary(insert, vals, null, null, false);
            var propertyList = vals.ToList();
            var setExpression = GetSetExpression(propertyList.Where(p => p.Key != keyCol.Name));
            var insertList = GetInsertList(propertyList);
            var valuesList = GetValuesList(propertyList);
            var sqlCmd = String.Format(InsertOrUpdateTemplate, tableName, keyCol.Name, setExpression, insertList,
                                       valuesList);

            Action tryact =
                () =>
                    {
                        using (var conn = new SqlOpenClose(connection))
                        {
                            CheckUseFederationFor(insert, connection);

                            conn.Cmd.CommandText = sqlCmd;

                            AddParameters(conn.Cmd.Parameters, propertyList);

                            ExecuteCommand(conn.Cmd);
                        }
                    };

            return ExecuteSqlActionWithRetries(tryact);

        }

        public static bool Delete<RecordType>(RecordType deleterec, SqlConnection connection, string tableName = null)
        {         
            Type recordTyp = typeof(RecordType);
            if (String.IsNullOrWhiteSpace(tableName))
                tableName = recordTyp.Name;

            var keyCol = GetPrimaryKey(recordTyp);
            if (keyCol == null)
                throw new ArgumentNullException(String.Format("no key column for type {0}", recordTyp.Name));

            Action tryact =
                () =>
                    {
                        using (var conn = new SqlOpenClose(connection))
                        {
                            CheckUseFederationFor(deleterec, connection);

                            conn.Cmd.CommandText = String.Format(DeleteTemplate, tableName, keyCol.Name);

                            var keyval = deleterec.GetPropertyVal(keyCol.Name);
                            AddParameter(conn.Cmd.Parameters, keyCol.Name, keyval);

                            ExecuteCommand(conn.Cmd);
                        }
                    };

            return ExecuteSqlActionWithRetries(tryact);

        }

        public static bool Get<RecordType>(RecordType fillrec, SqlConnection connection, string tableName = null)
        {
            Type recordTyp = typeof(RecordType);
            if (String.IsNullOrWhiteSpace(tableName))
                tableName = recordTyp.Name;

            var keyCol = GetPrimaryKey(recordTyp);
            if (keyCol == null)
                throw new ArgumentNullException(String.Format("no key column for type {0}", recordTyp.Name));

            var ret = false;
            Action tryact =
                () =>
                    {
                        using (var conn = new SqlOpenClose(connection))
                        {
                            CheckUseFederationFor(fillrec, connection);

                            conn.Cmd.CommandText = String.Format(GetTemplate, tableName, keyCol.Name);

                            var keyval = fillrec.GetPropertyVal(keyCol.Name);
                            AddParameter(conn.Cmd.Parameters, keyCol.Name, keyval);

                            ret = ExecuteSingleRead(fillrec, conn.Cmd);
                        }
                    };

            return ExecuteSqlActionWithRetries(tryact) && ret;
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

            command.CommandText = String.Format(
                Resources.DbUseFederation,
                federationInstance.FederationName, federationInstance.DistributionName, federationInstance.FedVal);

            ExecuteCommand(command);
        }

        private static void UseFederationRoot(SqlConnection connection)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = Resources.DbUseFederationRoot;
            ExecuteCommand(command);
        }

        public static bool ExecuteCommand(string sqlCmd, SqlConnection connection, object parameters = null)
        {
            Action tryact =
                () =>
                    {
                        using (var conn = new SqlOpenClose(connection))
                        {
                            var command = conn.Cmd;
                            command.CommandText = sqlCmd;
                            if (parameters != null)
                                AddParameters(command.Parameters, parameters);

                            ExecuteCommand(command);
                        }
                    };

            return ExecuteSqlActionWithRetries(tryact);
        }

        public static bool ExecuteCommandInRecordContext(object recordContext, string sqlCmd, SqlConnection connection, object parameters = null)
        {
            
            Action tryact =
                () =>
                    {
                        using (var conn = new SqlOpenClose(connection))
                        {
                            CheckUseFederationFor(recordContext, connection);

                            var command = conn.Cmd;
                            command.CommandText = sqlCmd;
                            if (parameters != null)
                                AddParameters(command.Parameters, parameters);

                            ExecuteCommand(command);
                        }
                    };

            return ExecuteSqlActionWithRetries(tryact);
        }

        public static IEnumerable<RecordType> Query<RecordType>(string command, SqlConnection connection, FederationInstance federationInstance, object parameters) where RecordType : new()
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
                            if (parameters != null)
                                AddParameters(conn.Cmd.Parameters, parameters);

                            ret = Query<RecordType>(conn.Cmd);
                        }
                    };

            ExecuteSqlActionWithRetries(tryact);
            return ret;
        }

        public static IEnumerable<FederationInfo> GetFederationInfo(SqlConnection connection)
        {
            return Query<FederationInfo>(Resources.DbGetFederationInfo, connection);
        }

        public static IEnumerable<FederationInstance> GetFederationInstances(SqlConnection connection)
        {
            return Query<FederationInfo>(Resources.DbGetFederationInfo, connection)
                .Select(fi => new FederationInstance()
                                  {
                                      FederationName = fi.Name,
                                      DistributionName = fi.DistributionName,
                                      FedVal = fi.RangeLow
                                  });
        }

        public static bool ExecuteCommandInRecordTypeContext(Type contextRecordTyp, string sqlCmd, 
            SqlConnection connection, object parameters = null)
        {
            var fedInfo = contextRecordTyp.GetFedInfo();
            if (fedInfo == null)
                return ExecuteCommand(sqlCmd, connection, parameters);

            var federations = GetFederationInstances(connection)
                    .Where(fi => fi.FederationName.Equals(fedInfo.FederationName));

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
                            if (!ExecuteCommandInRecordContext(fedOb, sqlCmd, shardconn, parameters))
                                loopstate.Stop();
                              
                          }
                      });

            return res.IsCompleted;
        }

        public static IEnumerable<RecordType> Query<RecordType>
            (string command, SqlConnection connection, object[] federationValues = null, object parameters = null) 
            where RecordType : new()
        {
            //if it isn't a federated type just perform normal query
            var contextRecordTyp = typeof (RecordType);
                var fedInfo = contextRecordTyp.GetFedInfo();
                if (fedInfo == null)
                    return Query<RecordType>(command, connection, (FederationInstance) null, parameters);

            //if it is a federated type check if federation shards are specified. If not
            //peform parallel queries across all federations
            var federations = federationValues != null && federationValues.Length > 0 ?
                 federationValues.Select(v => new FederationInstance() 
                    { FederationName = fedInfo.FederationName, DistributionName = fedInfo.DistributionName, FedVal = v})
                 :
                 GetFederationInfo(connection)
                    .Where(fi => fi.Name.Equals(fedInfo.FederationName))
                    .Select(fi => new FederationInstance()
                        {FederationName = fi.Name, 
                            DistributionName = fi.DistributionName, FedVal = fi.RangeLow});

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
                        foreach (var record in Query<RecordType>(command, taskconnection, fedShard, parameters))
                            ret.Enqueue(record);                         
                    }                
                });

            return ret;          
        }
        
        public static void AddParameters(SqlParameterCollection parameters, object source)
        {
            var propertyList = new Dictionary<string, object>();
            SerializeUtil.PropertiesToDictionary(source, propertyList);
            foreach (var keyValuePair in propertyList)
            {
                AddParameter(parameters, keyValuePair.Key, keyValuePair.Value);
            }
        }

        
        private static void AddParameters(SqlParameterCollection parameters, IEnumerable<KeyValuePair<string, object>> propertyList)
        {
            foreach (var keyValuePair in propertyList)
            {
                AddParameter(parameters, keyValuePair.Key, keyValuePair.Value);
            }
        }

        public static string GetDbTypeFor(Type type)
        {
             string dbtype;
             return !TypeToDbTypeDictionary.TryGetValue(type, out dbtype) ? null : dbtype;
        }

        private static void AddParameter(SqlParameterCollection parameters, string name, object value)
        {
            string dbtype;
            if (!TypeToDbTypeDictionary.TryGetValue(value.GetType(), out dbtype))
                throw new ArgumentException(String.Format("No mapping to dbtype for {0}", value.GetType()));
            var param = new SqlParameter("@" + name, value);

            if (TypeToUdtTypeDictionary.TryGetValue(value.GetType(), out dbtype))
                param.UdtTypeName = dbtype;

            parameters.Add(param);
        }

        private static string GetValuesList(IEnumerable<KeyValuePair<string, object>> propertyList)
        {
            var builder = new StringBuilder();

            foreach (var keyValuePair in propertyList)
            {
                if (builder.Length > 0)
                    builder.Append(',');

                builder.Append("@");
                builder.Append(keyValuePair.Key);
            }
            return builder.ToString();
        }

        private static string GetInsertList(IEnumerable<KeyValuePair<string, object>> propertyList)
        {
            var builder = new StringBuilder();

            foreach (var keyValuePair in propertyList)
            {
                if (builder.Length > 0)
                    builder.Append(',');

                builder.Append(keyValuePair.Key);
            }
            return builder.ToString();
        }

        private static string GetSetExpression(IEnumerable<KeyValuePair<string, object>> propertyList)
        {
            var builder = new StringBuilder();

            foreach (var keyValuePair in propertyList)
            {
                if (builder.Length > 0)
                    builder.Append(',');

                builder.Append(keyValuePair.Key);
                builder.Append(" = @");
                builder.Append(keyValuePair.Key);
            }
            return builder.ToString();
        }

        public static PropertyInfo GetPrimaryKey(Type source)
        {
            return SerializeUtil.GetPropertyWithAttribute(source, typeof (PrimaryKey));
        }

        private static object ConvertResult(SqlDataReader reader, int ordinal, Type needed)
        {
            var result = reader[ordinal];
            if (result == null)
                return null;

            if (needed == typeof(SqlXml))
                return reader.GetSqlXml(ordinal);

            //fixed with assembly binding
//            if (needed == typeof(SqlGeometry))
//                return SqlGeometry.Deserialize(reader.GetSqlBytes(ordinal));

            return ConvertVal(result, needed);
        }

        public static object ConvertVal(object result, Type needed)
        {
            if (result == null)
                return null;

            if (needed == result.GetType())
                return result;

            return Convert.ChangeType(result, needed);
        }

        private static void ResultSetToObjectProperties<RecordType>(RecordType loadRec, SqlDataReader reader)
        {
            var rectype = loadRec.GetType();
            for (var col = 0; col < reader.FieldCount; col++)
            {
                var colname = reader.GetName(col);
                var prop = rectype.GetProperty(colname);
                if (prop == null)
                    continue;

                prop.SetValue(loadRec, ConvertResult(reader, col, prop.PropertyType), null);
            }
        }

        private static IEnumerable<RecordType> Query<RecordType>(SqlCommand command) where RecordType : new()
        {
            IList<RecordType> ret = new List<RecordType>();
            command.CommandTimeout = 120;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var loadRec = new RecordType();
                    ResultSetToObjectProperties(loadRec, reader);
                    ret.Add(loadRec);
                }
            }

            return ret;
        }

        public static bool ExecuteSingleRead<RecordType>(RecordType loadRec, SqlCommand command)
        {
            command.CommandTimeout = 120;
            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                    return false;

                ResultSetToObjectProperties(loadRec, reader);

                return true;
            }
            
        }

        private static void ExecuteCommand(SqlCommand command)
        {
            command.CommandTimeout = 120;
            command.ExecuteNonQuery();
        }

        private static bool ExecuteSqlActionWithRetries(Action tryact)
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

        private static readonly HashSet<int> ConnectionRetryErrors = new HashSet<int>()
                                                                         {
                                                                            10054,// Connection reset by peer
                                                                            10053 //Software caused connection abort
                                                                         }; 
        private const int SqlRetryMax = 3;
        private static int HandleSqlException(SqlException ex, int retryNumber = 0)
        {

            if (ConnectionRetryErrors.Contains(ex.Number) && retryNumber < SqlRetryMax)
                return retryNumber + 1;

            var errorMessages = new StringBuilder("SQL Error ");
            if (retryNumber == SqlRetryMax)
                errorMessages.Append("giving up after retries");
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

            Trace.TraceError(errorMessages.ToString());
            return -1;
        }

        //{0} TableName
        //{1} IdCol
        //{2} SetExpression
        //{3} InsertColumnList
        //{4} ValuesList
        private static readonly string InsertOrUpdateTemplate = Resources.DbInsertOrUpdateRecord;

        //{0} TableName
        //{1} WhereExpression
        private static readonly string DeleteTemplate = Resources.DbDeleteRecord;

        //{0} TableName
        //{1} WhereExpression
        private static readonly string GetTemplate = Resources.DbGetRecord;
    }
}
