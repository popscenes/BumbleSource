using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using Dapper;
using Dark;
using NLog;
using Newtonsoft.Json;
using Website.Azure.Common.Sql.Infrastructure;
using Website.Infrastructure.Domain;

namespace Website.Azure.Common.Sql
{
    [AttributeUsage( AttributeTargets.Class)]
    public class ConfigureAggregateStorageAttribute : Attribute {}


//    public class AdoSchemaUpdate
//    {
//        public Action<Environment> UpdateSchema { get; set; }
//        public Action<Environment> RecreateSchema { get; set; }
//        public Action Configure { get; set; }
//
//        public static List<AdoSchemaUpdate> GetSchemaUpdatesFromAssembly(Assembly assembly)
//        {
//           var types =  assembly.GetTypes().Where(
//                type => type.GetCustomAttributes(true).Any(o => o is ConfigureAggregateStorageAttribute));
//
//            return types.Select(t =>
//            {
//                var update = t.GetMethod("UpgradeSchema", new[] {typeof (Environment)});
//                var recreate = t.GetMethod("RecreateSchema", new[] {typeof (Environment)});
//                var configure = t.GetMethod("Configure");
//                if (update == null || recreate == null || configure == null)
//                    throw new ArgumentException("need to have appropriate methods");
//
//
//                return new AdoSchemaUpdate()
//                            {
//                                UpdateSchema =
//                                    environment =>
//                                    update.Invoke(null, new object[] {environment}),
//                                RecreateSchema =
//                                    environment =>
//                                    recreate.Invoke(null, new object[] {environment}),
//                                Configure = () => configure.Invoke(null, new object[] {}),
//
//                            };
//            }).ToList();
//        }
//
//    }

    public static class ArmExtensions
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(ArmExtensions).Name);

        static ArmExtensions()
        {
            DapperExtensions.DapperExtensions.DefaultMapper = typeof(AggregateClassMapper<>);
        }

        /// <summary>
        /// This should only be used when the update is the source of truth, ie when replication from another source
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="json"></param>
        /// <param name="clrType"></param>
        /// <param name="transaction"> </param>
        public static void UpdateOrInsertAggregateRootFromJson(this IDbConnection dbConn, string json, string clrType, IDbTransaction transaction = null)
        {
            var type = Type.GetType(clrType);
            var obj = JsonConvert.DeserializeObject(json, type) as AggregateRootInterface;
            if(obj == null)
            {
                Logger.Warn("Failed to deserialize {0} from string \n {1}", clrType, json);
                return;
            }

            var trans = transaction ?? dbConn.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                var res = dbConn.Query<AggregateRootTable>(
                    string.Format(SelectForUpdateByAggregateId, AggregateMemberMap.GetEntityStorageTypeForAggregate(type).Name),
                    new { Id = obj.Id }, trans).SingleOrDefault();

                if (!ApplyAggregateUpdate(dbConn, obj, trans, res))
                {
                    trans.Rollback();
                    return;
                }

                trans.Commit();
            }
            catch (Exception e)
            {
                if (transaction == null)
                    trans.Rollback();
                throw;
            }

            if(transaction == null)
                trans.Dispose();

        }

        public static int CheckRemapTablesFor(this IDbConnection dbConn, List<Type> types, int batchSize = 100, IDbTransaction transaction = null)
        {
            return types.Sum(type => dbConn.CheckRemapTablesFor(type, batchSize, transaction));
        }
        
        public static int CheckRemapTablesFor(this IDbConnection dbConn, Type type, int batchSize = 100, IDbTransaction transaction = null)
        {
            var ret = 0;

            var trans = transaction ?? dbConn.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                var aggStoreRows = dbConn.Query<AggregateRootTable>(
                    string.Format(SelectForReindexByAggregateId, AggregateMemberMap.GetEntityStorageTypeForAggregate(type).Name),
                    new { MaxRows = batchSize }, trans);

                foreach (var row in aggStoreRows)
                {
                    var obj = JsonConvert.DeserializeObject(row.Json, type) as AggregateRootInterface;
                    if (obj == null)
                        throw new SerializationException(string.Format("Failed to deserialize {0} from string \n {1}", row.ClrType, row.Json));

                    if (!ApplyAggregateUpdate(dbConn, obj, trans, row))
                        throw new Exception(string.Format("Failed to apply update for {0}", row.Id));
                        
                }

                trans.Commit();
                ret = aggStoreRows.Count();
            }
            catch (Exception e)
            {
                if (transaction == null)
                    trans.Rollback();
                trans.Rollback();
                throw;
            }
            

            if (transaction == null)
                trans.Dispose();
 
            return ret;
        }

        public static void InsertAggregateRoot<T>(this IDbConnection dbConn, T aggregateRoot, IDbTransaction transaction = null)
            where T : AggregateRootInterface, new()
        {
            var trans = transaction ?? dbConn.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                if (string.IsNullOrWhiteSpace(aggregateRoot.Id))
                    aggregateRoot.Id = Guid.NewGuid().ToString();
                if (string.IsNullOrWhiteSpace(aggregateRoot.FriendlyId))
                    aggregateRoot.FriendlyId = aggregateRoot.Id;
//                    aggregateRoot.Id =
//                        dbConn.Query<string>(string.Format(GetNextAggregateId, typeof (T).Name), null, trans).Single();
                
                if (!ApplyAggregateUpdate(dbConn, aggregateRoot, trans))
                {
                    trans.Rollback();
                    return;
                }

                trans.Commit();
            }
            catch (Exception e)
            {
                if (transaction == null)
                    trans.Rollback();
                throw;
            }

            if (transaction == null)
                trans.Dispose();
            
        }

        public static void UpdateAggregateRoot<T>(this IDbConnection dbConn, string id, Action<T> aggregateRootAction = null, IDbTransaction transaction = null)
            where T : class, AggregateRootInterface, new()
        {
            var trans = transaction ?? dbConn.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                var res = dbConn.Query<AggregateRootTable>(
                    string.Format(SelectForUpdateByAggregateId, AggregateMemberMap.GetEntityStorageTypeForAggregate<T>().Name),
                    new {Id = id}, trans).SingleOrDefault();

                if (res == null)
                {
                    Logger.Warn("Failed to find aggregate root {1}", id);
                    trans.Rollback();
                    return;
                }

                var obj = JsonConvert.DeserializeObject(res.Json, Type.GetType(res.ClrType)) as T;
                if(aggregateRootAction != null)
                    aggregateRootAction(obj);


                if (!ApplyAggregateUpdate(dbConn, obj, trans, res))
                {
                    trans.Rollback();
                    return;
                }

                trans.Commit();
            }
            catch (Exception e)
            {
                if (transaction == null)
                    trans.Rollback();
                throw;
            }

            if (transaction == null)
                trans.Dispose();

            
        }


        public static T GetAggregateRoot<T>(this IDbConnection dbConn, string id, IDbTransaction transaction = null)
            where T : class, AggregateRootInterface, new()
        {

            var res = dbConn.GetAggregateRootStorageTable<T, AggregateRootTable>(id, transaction);

            if (res == null) return null;

            return JsonConvert.DeserializeObject(res.Json, Type.GetType(res.ClrType)) as T;
        }

        public static IEnumerable<T> GetAggregateRoots<T>(this IDbConnection dbConn, IDbTransaction transaction = null)
                where T : class, AggregateRootInterface, new()
        {

            var res = dbConn.GetAggregateRootStorageTables<T, AggregateRootTable>(transaction);

            return res == null ? null : res.Select(r => JsonConvert.DeserializeObject(r.Json, Type.GetType(r.ClrType)) as T);
        }

        public static IEnumerable<T> QueryAggregates<T>(this IDbConnection dbConn, string cmd, object parameters = null, CommandType? commandType = null,
            IDbTransaction transaction = null)
                where T : class, AggregateRootInterface, new()
        {

            var res = dbConn.Query<AggregateRootTable>(cmd, parameters, commandType: commandType, transaction: transaction);

            return res.Select(table => JsonConvert.DeserializeObject(table.Json, Type.GetType(table.ClrType)) as T);
        }


        public static TStoreType GetAggregateRootStorageTable<TAggType, TStoreType>(this IDbConnection dbConn, string id, IDbTransaction transaction = null)
            where TStoreType : AggregateRootTable
            where TAggType : class, AggregateRootInterface
        {
            TStoreType res = null;
 
            res =
                dbConn.Query<TStoreType>(
                    string.Format(SelectForReadByAggregateId,
                                    AggregateMemberMap.GetEntityStorageTypeForAggregate<TAggType>().Name)
                                    , new { Id = id }, transaction:transaction).SingleOrDefault();

            return res;
        }

        public static IEnumerable<TStoreType> GetAggregateRootStorageTables<TAggType, TStoreType>(this IDbConnection dbConn, IDbTransaction transaction = null)
            where TStoreType : AggregateRootTable
            where TAggType : class, AggregateRootInterface
        {
            var res =
                dbConn.Query<TStoreType>(
                    string.Format(SelectAllForRead,
                                    AggregateMemberMap.GetEntityStorageTypeForAggregate<TAggType>().Name)
                                    , null, transaction: transaction);

            return res;
        }

        public static IDbConnection OpenDbConnection(this string connectionString)
        {
            var conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        
        public static void UpgradeSchemaFromAssembly(this IDbConnection dbConn, Assembly assembly)
        {
            dbConn.RunSqlRes(assembly, "create_table");
            dbConn.RunSqlRes(assembly, "stored_proc");
        }

        public static void RunSqlRes(this IDbConnection dbConn, Assembly assembly, string resourceLike)
        {
            var resources = assembly.GetManifestResourceNames();

            var list = (from resource in resources
                        select new ResourceSet(assembly.GetManifestResourceStream(resource))
                        into set
                        from entry
                            in set.OfType<DictionaryEntry>()
                            .Where(entry => entry.Key.ToString().Contains(resourceLike))
                        select entry.Value as string
                       ).ToList();

            foreach (var sql in list)
            {
                dbConn.RunSqlScript(sql);
            }
        }
            
        public static void RunSqlScript(this IDbConnection dbConn, string statements)
        {
            foreach (var sqlStatement in statements.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
            {
                dbConn.Execute(sqlStatement);
            }
        }

        private static bool ApplyAggregateUpdate<T>(IDbConnection dbConn, T obj, IDbTransaction trans, AggregateRootTable existing = null)
                where T : AggregateRootInterface
        {
            var visitor = new MappedPropertyVisitor(AggregateMemberMap.TypeMap);
            var tableRows = visitor.Visit(obj);
            if (tableRows.Count == 0 || existing != null && tableRows.OfType<AggregateRootTable>().First().JsonHash == existing.JsonHash)
                return false;


            var tableTypes = obj.GetRowTypesForDelete();
            foreach (var deleteTable in tableTypes)
            {
                dbConn.Execute(string.Format((string)DeleteByAggregateId, (object)deleteTable.Name)
                               , new { Id = obj.Id }, trans);
            }

            foreach (dynamic tableRow in tableRows)
            {
                if (tableRow is AggregateRootTable && existing != null)
                {
                    tableRow.RowId = existing.RowId;
                    DapperExtensions.DapperExtensions.Update(dbConn, tableRow, trans);
                }
                else
                    DapperExtensions.DapperExtensions.Insert(dbConn, tableRow, trans);
            }
            return true;
        }

        private const string SelectAllForRead = "SELECT * FROM {0}";
        private const string SelectForReadByAggregateId = "SELECT * FROM {0} WHERE Id=@Id";
        private const string SelectForUpdateByAggregateId = "SELECT * FROM {0} with (updlock, rowlock) WHERE Id=@Id";
        private const string SelectForReindexByAggregateId = "SELECT TOP (@MaxRows) * FROM {0} with (updlock, rowlock) WHERE JsonHash < 0";
        private const string DeleteByAggregateId = "DELETE FROM {0} with (updlock, rowlock) WHERE Id=@Id";

        private const string GetNextAggregateId = "select '{0}s/' + Convert(varchar(256), NEXT VALUE FOR {0}s)";


    }
}