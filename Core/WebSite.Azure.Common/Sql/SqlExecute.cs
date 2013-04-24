using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SqlServer.Types;
using Microsoft.WindowsAzure.ServiceRuntime;
using Website.Azure.Common.Properties;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Util;
using Website.Infrastructure.Util.Extension;

namespace Website.Azure.Common.Sql
{
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

            prop.SetValue(fedObject, SerializeUtil.ConvertVal(fedVal, prop.PropertyType), null);
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
                FedVal = null,
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

        public const string DbString = "nvarchar(MAX)";
        public const string DbGuid = "uniqueidentifier";
        public const string DbInt = "int";
        public const string DbFloat = "float";
        public const string DbDateTimeOffset = "datetimeoffset(7)";
        public const string DbXml = "xml";
        public const string DbGeography = "geography";
        public const string DbLong = "bigint";
        public const string DbDateTime = "datetime2";

        public static readonly Dictionary<Type, string> TypeToDbColTypeDictionary
            = new Dictionary<Type, string>()
                  {
                      {typeof(string), DbString},
                      {typeof(Guid), DbGuid},
                      {typeof(int), DbInt},
                      {typeof(double), DbFloat},
                      {typeof(DateTimeOffset), DbDateTimeOffset},
                      {typeof(SqlXml), DbXml},
                      {typeof(SqlGeography), DbGeography},  
                      {typeof(long), DbLong},  
                      {typeof(DateTime), DbDateTime},                    
                  };

        public static readonly Dictionary<Type, SqlDbType> TypeToDbTypeDictionary
    = new Dictionary<Type, SqlDbType>()
                  {
                      {typeof(string), SqlDbType.NVarChar},
                      {typeof(Guid), SqlDbType.UniqueIdentifier},
                      {typeof(int), SqlDbType.Int},
                      {typeof(double), SqlDbType.Float},
                      {typeof(DateTimeOffset), SqlDbType.DateTimeOffset},
                      {typeof(DateTime), SqlDbType.DateTime2},
                      {typeof(SqlXml), SqlDbType.Xml},
                      {typeof(SqlGeography), SqlDbType.Udt},  
                      {typeof(long), SqlDbType.BigInt},                                            
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
                bool.TryParse(Config.Instance.GetSetting("DisableFederation"), 
                out ret);
                _federationDisabled = ret;
                return ret;
            }
        }
        public static string GetConnectionStringFromConfig(string settingName, string dbName = null)
        {
            var connectionString = Config.Instance.GetSetting(settingName);

            if (String.IsNullOrWhiteSpace(dbName))
                return connectionString;


            var csBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = dbName };
            return csBuilder.ToString();
        }

        public static bool InsertOrUpdateAll<RecordType>(IEnumerable<RecordType> insert, SqlConnection connection,
                                                      string tableName = null)
        {
            return insert.Aggregate(false, (b, record) => InsertOrUpdate(record, connection, tableName) || b);
        }

        public static bool InsertOrUpdate<RecordType>(RecordType insert, SqlConnection connection, string tableName = null)
        {
            Type recordTyp = typeof (RecordType);
            if(String.IsNullOrWhiteSpace(tableName))
                tableName = recordTyp.Name;
            
            var keyCol = GetPrimaryKey(recordTyp);
            if(keyCol == null || keyCol.Count == 0)
                throw new ArgumentNullException(String.Format("no key column for type {0}", recordTyp.Name));


            var vals = new Dictionary<string, object>();
            SerializeUtil.PropertiesToDictionary(insert, vals, null, null, false);
            var propertyList = vals.ToList();
            var setExpression = GetSetExpression(propertyList);
            var insertList = GetInsertList(propertyList);
            var valuesList = GetValuesList(propertyList);
            var keyPropList = AddParametersEqText("", keyCol, true);
            var sqlCmd = String.Format(InsertOrUpdateTemplate, tableName, keyPropList, setExpression, insertList,
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

        public static bool DeleteAll<RecordType>(IEnumerable<RecordType> deleterec, SqlConnection connection, string tableName = null)
        {
            return deleterec.Aggregate(false, (b, record) => Delete(record, connection, tableName) || b);            
        }

        public static bool Delete<RecordType>(RecordType deleterec, SqlConnection connection, string tableName = null)
        {
            return DeleteBy(GetPrimaryKey(typeof(RecordType)), deleterec, connection, tableName);
        }

        public static bool DeleteBy<RecordType>(string propertyName, RecordType deleterec, SqlConnection connection, string tableName = null)
        {
            var recordTyp = typeof(RecordType);
            return DeleteBy(new List<PropertyInfo>(){recordTyp.GetProperty(propertyName)}, deleterec, connection, tableName);
        }

        public static bool DeleteBy<RecordType>(IEnumerable<RecordType> deleterec, SqlConnection connection, params Expression<Func<RecordType, object>>[] propertyExpressions)
        {
            return deleterec.Aggregate(false, (b, record) => DeleteBy(record, connection, propertyExpressions) || b);  
        }

        public static bool DeleteBy<RecordType>(RecordType deleterec, SqlConnection connection, params Expression<Func<RecordType, object>>[] propertyExpressions )
        {
            return DeleteBy(propertyExpressions.Select(expression => expression.ToPropertyInfo()).ToList(), deleterec, connection);
        }

        private static bool DeleteBy<RecordType>(IList<PropertyInfo> properties, RecordType deleterec, SqlConnection connection, string tableName = null)
        {
            Type recordTyp = typeof(RecordType);
            if (String.IsNullOrWhiteSpace(tableName))
                tableName = recordTyp.Name;

            if (properties == null)
                throw new ArgumentNullException(String.Format("no key column for type {0}", recordTyp.Name));

            Action tryact =
                () =>
                {
                    using (var conn = new SqlOpenClose(connection))
                    {
                        CheckUseFederationFor(deleterec, connection);

                        conn.Cmd.CommandText = String.Format(DeleteTemplate, tableName);
                        AddParametersEq(conn.Cmd, deleterec, properties);                           
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

            var keyCols = GetPrimaryKey(recordTyp);
            if (keyCols == null)
                throw new ArgumentNullException(String.Format("no key column for type {0}", recordTyp.Name));

            var ret = false;
            Action tryact =
                () =>
                    {
                        using (var conn = new SqlOpenClose(connection))
                        {
                            CheckUseFederationFor(fillrec, connection);

                            conn.Cmd.CommandText = String.Format(GetTemplate, tableName);
                            AddParametersEq(conn.Cmd, fillrec, keyCols);                           
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
                federationInstance.FederationName, federationInstance.DistributionName, federationInstance.FedVal.ToEqualExpression());

            ExecuteCommand(command);
        }

        private static void UseFederationRoot(SqlConnection connection)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = Resources.DbUseFederationRoot;
            ExecuteCommand(command);
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
                            
                            if(isStoredProc)
                                command.CommandType = CommandType.StoredProcedure;
                            
                            if (parameters != null)
                                AddParameters(command.Parameters, parameters);

                            ExecuteCommand(command);
                        }
                    };

            return ExecuteSqlActionWithRetries(tryact);
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
                            
                            if (parameters != null)
                                AddParameters(command.Parameters, parameters);

                            ExecuteCommand(command);
                        }
                    };

            return ExecuteSqlActionWithRetries(tryact);
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
                            
                            if (parameters != null)
                                AddParameters(conn.Cmd.Parameters, parameters);

                            ret = Query<RecordType>(conn.Cmd);
                        }
                    };

            ExecuteSqlActionWithRetries(tryact);
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
            var contextRecordTyp = typeof (RecordType);
                var fedInfo = contextRecordTyp.GetFedInfo();
                if (fedInfo == null)
                    return Query<RecordType>(command, connection, (FederationInstance) null, parameters, isStoredProc);

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

        public static void AddParametersEq(SqlCommand query, object source, IList<PropertyInfo> properties)
        {
            query.CommandText = AddParametersEqText(query.CommandText, properties);
            AddParameters(query.Parameters, source, properties);
        }

        private static string AddParametersEqText(string query, IEnumerable<PropertyInfo> properties, bool noWhere = false)
        {
            var builder = new StringBuilder(query);
            bool first = true;
            var where = noWhere || query.ToLower().Contains("where") ? "" : " where ";
            foreach (var propertyInfo in properties)
            {
                builder.Append(!first ? " and " : where);
                first = false;
                builder.Append(string.Format(PropEq, propertyInfo.Name));
            }

            return builder.ToString();
        }

        private static void AddParameters(SqlParameterCollection queryParams, object source, IEnumerable<PropertyInfo> properties = null)
        {
            if (properties == null)
                properties = source.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var keyval = source.GetPropertyVal(prop.Name);
                AddParameter(queryParams, prop.Name, keyval);
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
             return !TypeToDbColTypeDictionary.TryGetValue(type, out dbtype) ? null : dbtype;
        }

        private static void AddParameter(SqlParameterCollection parameters, string name, object value)
        {
            if (value == null)
            {
                parameters.AddWithValue(name, DBNull.Value);
                return;
            }
                
            string dbtype = null;
            if (value != null && !TypeToDbColTypeDictionary.TryGetValue(value.GetType(), out dbtype) && !(value is Type))
                throw new ArgumentException(String.Format("No mapping to dbtype for {0}", value.GetType()));
        
            var param = (value is Type)
                ? new SqlParameter("@" + name, TypeToDbTypeDictionary[Nullable.GetUnderlyingType(value as Type) ?? value as Type])
                : new SqlParameter("@" + name, value);


            if (value != null && TypeToUdtTypeDictionary.TryGetValue(value.GetType(), out dbtype))
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

                builder.Append(string.Format(PropEq, keyValuePair.Key));
            }
            return builder.ToString();
        }

        public static IList<PropertyInfo> GetPrimaryKey(Type source)
        {
            var ret = SerializeUtil.GetPropertiesWithAttribute(source, typeof(PrimaryKey));

            var fedProp = SerializeUtil.GetPropertyWithAttribute(source, typeof(FederationCol));
            FederationCol fedAtt = null;
            if (fedProp == null)
                return ret;
            
            fedAtt = fedProp.GetCustomAttributes(true).First(a => a.GetType() == typeof(FederationCol)) as FederationCol;
            if (fedAtt != null && !fedAtt.IsReferenceTable)
                ret.Add(fedProp);

            return ret;
        }

        public static IList<PropertyInfo> GetSingleColIndexes(Type source)//todo add multicol that look at att on class
        {
            return SerializeUtil.GetPropertiesWithAttribute(source, typeof(SqlIndex));
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

            return SerializeUtil.ConvertVal(result, needed);
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

        //{0} Property/Col name
        private static readonly string PropEq = Resources.DbColEqProp;
    }
}
