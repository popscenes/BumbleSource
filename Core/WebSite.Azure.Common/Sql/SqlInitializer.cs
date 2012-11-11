using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Microsoft.SqlServer.Types;
using Microsoft.WindowsAzure.ServiceRuntime;
using Website.Azure.Common.Binding;
using Website.Infrastructure.Util;

namespace Website.Azure.Common.Sql
{

    public class SqlInitializer : IDisposable
    {
        private readonly SqlConnection _connection;

        public SqlInitializer()
        {
            _connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString"));
        }

        public SqlInitializer(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            
        }

        public void CreateDb(string databasename)
        {
            if(HasDb(databasename))
                return;
            string cmdText = String.Format(
                Properties.Resources.DbCreate
                , databasename);
           
            SqlExecute.ExecuteCommand(cmdText, _connection);
        }


        public static bool CreateFederationFor(Type recordTyp, SqlConnection connection)
        {
            var prop = SerializeUtil.GetPropertyWithAttribute(recordTyp, typeof (FederationCol));
            if (prop == null || SqlExecute.FederationDisabled)
                return false;
            var fedAtt = prop.GetCustomAttributes(true).First(a => a.GetType() == typeof (FederationCol)) as FederationCol;

            var ret = SqlExecute.GetFederationInfo(connection);
            if (ret.Any(fi => fi.Name.Equals(fedAtt.FederationName, StringComparison.CurrentCultureIgnoreCase)))
                return true;

            var dbTypeFor = SqlExecute.GetDbTypeFor(prop.PropertyType);
            if (dbTypeFor == null)
                return false;

            string cmdText = String.Format(
                Properties.Resources.DbCreateFederation
                , fedAtt.FederationName, fedAtt.DistributionName, dbTypeFor);

            return SqlExecute.ExecuteCommand(cmdText, connection);
        }

        public static bool DeleteFederationFor(Type recordTyp, SqlConnection connection)
        {
            var prop = SerializeUtil.GetPropertyWithAttribute(recordTyp, typeof(FederationCol));
            if (prop == null || SqlExecute.FederationDisabled)
                return false;
            var fedAtt = prop.GetCustomAttributes(true).First(a => a.GetType() == typeof(FederationCol)) as FederationCol;

            var ret = SqlExecute.GetFederationInfo(connection);
            if (!ret.Any(fi => fi.Name.Equals(fedAtt.FederationName, StringComparison.CurrentCultureIgnoreCase)))
                return true;

            string cmdText = String.Format(
                Properties.Resources.DbDeleteFederation
                , fedAtt.FederationName);

            return SqlExecute.ExecuteCommand(cmdText, connection);
        }

        public void DeleteDb(string databasename)
        {
            if (!HasDb(databasename))
                return;

            string cmdDropConnections =
                String.Format(
                Properties.Resources.DbDropConnections,
                databasename);

            string cmdText = String.Format(
                Properties.Resources.DbDrop, 
                databasename);

            
            //SqlExecute.ExecuteCommand(cmdDropConnections, _connection, string.Format("drop connections {0}", databasename));
            SqlExecute.ExecuteCommand(cmdText, _connection);
        }

        public bool HasDb(string databasename)
        {
            var cmdText = String.Format(
                Properties.Resources.DbExists,
                databasename); 
            var res = SqlExecute.Query<CountResult>(cmdText, _connection).SingleOrDefault();
            return res.Count > 0;
        }

        public static bool CreateTableFrom(Type metaTyp, SqlConnection connection, string tableName = null)
        {
            if(string.IsNullOrWhiteSpace(tableName))
                tableName = metaTyp.Name;

            CreateFederationFor(metaTyp, connection);

            var keyProp = SqlExecute.GetPrimaryKey(metaTyp);
            if (keyProp == null)
                throw new ArgumentException(string.Format("No primary key attribute for {0}", tableName));

            if (!CreateTable(tableName, metaTyp, connection))
                return false;

            if (metaTyp.GetProperties()
                .Where(prop => SqlExecute.TypeToDbTypeDictionary.ContainsKey(prop.PropertyType))
                .Where(prop => prop.GetIndexParameters().Length <= 0)
                .Any(prop => !(CreateColumnFrom(prop, connection, tableName) && AddConstraintsFrom(prop, connection, tableName))))
            {
                return false;
            }

            return CreateIndexesFor(metaTyp, connection, tableName);
        }

        private static bool CreateIndexesFor(Type metaTyp, SqlConnection connection, string tableName = null)
        {
            var indexes = SqlExecute.GetSingleColIndexes(metaTyp);
            if (indexes.Count == 0)
                return true;
            return (from propertyInfo in indexes
                        let indexInfo = propertyInfo.GetCustomAttributes(true).First(a => a.GetType() == typeof (SqlIndex)) as SqlIndex
                        where indexInfo != null
                        let unique = indexInfo.Unique ? "UNIQUE" : ""
                        let clustered = indexInfo.Clustered ? "" : "NON"
                        let table = tableName ?? metaTyp.Name
                        let column = propertyInfo.Name
                        select string.Format(Properties.Resources.DbCreateSingleColIndex, unique, clustered, table, column))
                            .Aggregate(true, 
                                (current, sqlCmd) => 
                                    current &&
                                    SqlExecute.ExecuteCommandInRecordTypeContext(metaTyp, sqlCmd, connection));
        }

        private static bool IsNotNullable(object attribute)
        {
            return attribute.GetType() == typeof (PrimaryKey)
                   || attribute.GetType() == typeof (NotNullable)
                   || attribute.GetType() == typeof (FederationCol);
        }

        private static string ColumnTextFor(PropertyInfo prop)
        {
            if(!SqlExecute.TypeToDbTypeDictionary.ContainsKey(prop.PropertyType))
                throw new ArgumentException("no mapping for property type");

            var type = SqlExecute.TypeToDbTypeDictionary[prop.PropertyType];

            if (type == SqlExecute.DbString && SerializeUtil.HasAttribute(prop, typeof(PrimaryKey)))
                type = type.Replace("MAX", "255");

            var name = prop.Name;
            var constraints = "NULL";
            if (prop.GetCustomAttributes(false).Any(IsNotNullable))
                constraints = "NOT NULL";

            return string.Format("{0} {1} {2}", name, type, constraints);
        }

        private static bool AddConstraintsFrom(PropertyInfo prop, SqlConnection connection, string tableName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                tableName = prop.DeclaringType.Name;

            var ret = true;

            if (SerializeUtil.HasAttribute(prop, typeof(SpatialIndex)))
            {
                string sqlCmd = string.Format(
                    Properties.Resources.DbCreateSpatialIndex, tableName, prop.Name);
                ret = SqlExecute.ExecuteCommandInRecordTypeContext(prop.DeclaringType,
                    sqlCmd, connection) && ret;
            }

            return ret;
        }

        private static bool CreateColumnFrom(PropertyInfo prop, SqlConnection connection, string tableName = null)
        {
            if(string.IsNullOrWhiteSpace(tableName))
                tableName = prop.DeclaringType.Name;


            var colText = ColumnTextFor(prop);
            var sqlCmd = string.Format(
                Properties.Resources.DbAddColumn
                    ,tableName , prop.Name, colText
                );

            return SqlExecute.ExecuteCommandInRecordTypeContext
                (prop.DeclaringType, sqlCmd, connection);
        }


        private static bool CreateTable(string tableName, Type metaTyp, SqlConnection connection)
        {
            var keyProp = SqlExecute.GetPrimaryKey(metaTyp);
            var colList = ColumnTextFor(keyProp);
            var keyList = keyProp.Name;

            var fedProp = SerializeUtil.GetPropertyWithAttribute(metaTyp, typeof(FederationCol));
            FederationCol fedAtt = null;
            if (fedProp != null && !SqlExecute.FederationDisabled)
            {
                fedAtt = fedProp.GetCustomAttributes(true).First(a => a.GetType() == typeof(FederationCol)) as FederationCol;
                if (fedAtt != null && !fedAtt.IsReferenceTable)
                {
                    colList += "," + ColumnTextFor(fedProp);
                    keyList += "," + fedProp.Name;    
                }
                
            }

            var sqlCmd = string.Format(
                Properties.Resources.DbCreateTable
                , tableName, colList, keyList);

            if(fedAtt != null)
            {
                if(!fedAtt.IsReferenceTable)
                    sqlCmd += string.Format(Properties.Resources.DbFederatedOn, fedAtt.DistributionName, fedProp.Name);

                //this should create the default federation key
                var instanceForDefaultFed = Activator.CreateInstance(metaTyp, true);
                return SqlExecute.ExecuteCommandInRecordContext(instanceForDefaultFed,
                    sqlCmd, connection);
            }
                
            return SqlExecute.ExecuteCommand(sqlCmd, connection);
        }



        public bool DeleteTable(string tableName, SqlConnection connection)
        {
            var sqlCmd = string.Format(
                Properties.Resources.DbDeleteTable
                , tableName);

            return SqlExecute.ExecuteCommand(sqlCmd, connection);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
