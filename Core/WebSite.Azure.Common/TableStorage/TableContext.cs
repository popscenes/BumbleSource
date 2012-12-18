using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Website.Azure.Common.DataServices;
using Website.Azure.Common.Environment;

namespace Website.Azure.Common.TableStorage
{
    public interface TableContextInterface
    {
        void InitTable<TableEntryType>(string tableName);

        IQueryable<TableEntryType> PerformQuery<TableEntryType>(string tableName
                , Expression<Func<TableEntryType, bool>> query = null
                , int partition = 0, int take = -1);

        IQueryable<TableEntryType> PerformParallelQueries<TableEntryType>(string tableName
                , Expression<Func<TableEntryType, bool>>[] queries
                , int partition = 0, int take = -1);

        IQueryable<SelectType> PerformSelectQuery<TableEntryType, SelectType>(string tableName
                , Expression<Func<TableEntryType, bool>> query
                , Expression<Func<TableEntryType, SelectType>> selectExpression
                , int partition = 0, int take = -1);

        IQueryable<SelectType> PerformParallelSelectQueries<TableEntryType, SelectType>(string tableName
                , Expression<Func<TableEntryType, bool>>[] queries
                , Expression<Func<TableEntryType, SelectType>> selectExpression
                , int partition = 0, int take = -1);

        DataServiceResponse SaveChanges();
        DataServiceResponse SaveChanges(SaveChangesOptions saveChangesOptions);
        void Delete<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>> query, int partition = 0);
        void Delete(StorageTableEntryInterface tableEntry);
        void Store(string tableName, StorageTableEntryInterface tableEntry);
    }

    public class TableContext : TableContextInterface
    {
        private readonly CloudStorageAccount _account;
        private readonly TableServiceContext _containedContext;

        public TableContext(CloudStorageAccount account)
        {
            _account = account;
            var client = new CloudTableClient(new Uri(account.TableEndpoint.AbsoluteUri), account.Credentials);
            _containedContext = new TableServiceContext(client);
            _containedContext.WritingEntity += OnWritingEntity;
            _containedContext.ReadingEntity += OnReadingEntity;
        }

        public void InitTable<TableEntryType>(string tableName)
        {
            var cli = _account.CreateCloudTableClient();


            var peopleTable = cli.GetTableReference(tableName);

            if (peopleTable.Exists())
                return;

            Func<bool> create =
                () =>
                    {
                        peopleTable.CreateIfNotExists();
                        SaveChanges();
                        return true;
                    };
            DataServicesQueryHelper.QueryRetry(create);

            var initOb =
                Activator.CreateInstance(typeof(TableEntryType), true) as TableServiceEntity;
            if (initOb == null)
                throw new ArgumentException(
                    "Entity type must derive from TableServiceEntity");
            initOb.PartitionKey = "123";
            initOb.RowKey = "123";

            Func<bool> add =
                () =>
                    {
                        _containedContext.AddObject(tableName, initOb);
                        SaveChanges();
                        return true;
                    };
            DataServicesQueryHelper.QueryRetry(add);

            Func<bool> delete =
                () =>
                    {
                        _containedContext.DeleteObject(initOb);
                        SaveChanges();
                        return true;
                    };
            DataServicesQueryHelper.QueryRetry(delete);
            
        }

        public IQueryable<TableEntryType> PerformQuery<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>> query = null, int partition = 0, int take = -1)
        {
            Func<IQueryable<TableEntryType>> exec = () =>
                                                        {
                                                            var execQuery =
                                                                _containedContext.CreateQuery<TableEntryType>(tableName) as
                                                                IQueryable<TableEntryType>;

                                                            if (query != null)
                                                                execQuery = execQuery.Where(query);

                                                            return ExecuteQuery(execQuery, take);
                                                        };

            var ret = DataServicesQueryHelper.QueryRetry(exec);
            return ret ?? (new List<TableEntryType>()).AsQueryable();
        }

        public IQueryable<TableEntryType> PerformParallelQueries<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>>[] queries, int partition = 0, int take = -1)
        {
            var res = new ConcurrentQueue<TableEntryType>();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(8, queries.Length) };
            Action<Expression<Func<TableEntryType, bool>>> parallelAction =
                (q) =>
                    {
                        var items = PerformQuery(tableName, q, partition, take);
                        foreach (var tableEntry in items)
                        {
                            res.Enqueue(tableEntry);
                        }
                    };


            Parallel.ForEach(queries, parallelOptions, parallelAction);
            return res.AsQueryable();
        }

        public IQueryable<SelectType> PerformSelectQuery<TableEntryType, SelectType>(string tableName, Expression<Func<TableEntryType, bool>> query, Expression<Func<TableEntryType, SelectType>> selectExpression, int partition = 0, int take = -1)
        {
            //just fake it if not using real storage
            //throw new NotSupportedException("as at at 1.6 sdk Development storage doesn't fucking support projection queries");

            if (!AzureEnv.UseRealStorage)
            {
                Func<IQueryable<TableEntryType>> exec = () =>
                                                            {
                                                                var execQuery =
                                                                    _containedContext.CreateQuery<TableEntryType>(tableName) as
                                                                    IQueryable<TableEntryType>;
                                                                execQuery = execQuery.Where(query);
                                                                return ExecuteQuery(execQuery, take);
                                                            };

                var ret = DataServicesQueryHelper.QueryRetry(exec);
                return ret.Select(selectExpression);
            }
            else
            {
                Func<IQueryable<SelectType>> exec = () =>
                                                        {
                                                            var execQuery =
                                                                _containedContext.CreateQuery<TableEntryType>(tableName) as
                                                                IQueryable<TableEntryType>;
                                                            execQuery = execQuery.Where(query);
                                                            return ExecuteQuery(execQuery.Select(selectExpression), take);
                                                        };

                var ret = DataServicesQueryHelper.QueryRetry(exec);
                return ret ?? (new List<SelectType>()).AsQueryable();
            }
        }

        public IQueryable<SelectType> PerformParallelSelectQueries<TableEntryType, SelectType>(string tableName, Expression<Func<TableEntryType, bool>>[] queries, Expression<Func<TableEntryType, SelectType>> selectExpression, int partition = 0, int take = -1)
        {
            var res = new ConcurrentQueue<SelectType>();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(8, queries.Length) };
            Action<Expression<Func<TableEntryType, bool>>> parallelAction =
                (q) =>
                    {
                        var items = PerformSelectQuery(tableName, q, selectExpression, partition, take);
                        foreach (var tableEntry in items)
                        {
                            res.Enqueue(tableEntry);
                        }
                    };

            Parallel.ForEach(queries, parallelOptions, parallelAction);
            return res.AsQueryable();
        }

        public DataServiceResponse SaveChanges()
        {
            return SaveChanges(SaveChangesOptions.ReplaceOnUpdate);
        }

        public DataServiceResponse SaveChanges(SaveChangesOptions saveChangesOptions)
        {
            return _containedContext.SaveChangesWithRetries(saveChangesOptions);
        }

        public void Delete<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>> query, int partition = 0)
        {
            var entities = PerformQuery(tableName, query, partition);
            foreach (var tableEntity in entities)
            {
                _containedContext.DeleteObject(tableEntity);
            }
        }

        public void Delete(StorageTableEntryInterface tableEntry)
        {
            if (_containedContext.GetEntityDescriptor(tableEntry) != null)
            {
                _containedContext.DeleteObject(tableEntry);
            }
        }

        public void Store(string tableName, StorageTableEntryInterface tableEntry)
        {
            if (tableEntry.KeyChanged && _containedContext.GetEntityDescriptor(tableEntry) != null)
            {
                _containedContext.DeleteObject(tableEntry);
                this.SaveChanges();
            }

            //do insert or replace in prod env
            if (AzureEnv.UseRealStorage)
            {
                if (_containedContext.GetEntityDescriptor(tableEntry) == null)
                    _containedContext.AttachTo(tableName, tableEntry);
                
                _containedContext.UpdateObject(tableEntry);
            }
            else
            {
                if (_containedContext.GetEntityDescriptor(tableEntry) == null)
                    _containedContext.AddObject(tableName, tableEntry);        
                else
                    _containedContext.UpdateObject(tableEntry);
                
            }
            
        }

        private  IQueryable<TableEntryType> ExecuteQuery<TableEntryType>(IQueryable<TableEntryType> query, int take = -1)
        {
            var ret = query.AsTableServiceQuery(_containedContext);
            if (take > 0)
                ret = ret.Take(take).AsTableServiceQuery(_containedContext);

            return ret.Execute().ToList()//cause evaluation of enumeration to occur;
                .AsQueryable();
        }

        private void OnWritingEntity(object sender, ReadingWritingEntityEventArgs args)
        {
            var extendableEntity = args.Entity as ExtendableTableEntry;
            if (extendableEntity != null)
            {
                OnWritingEntityExtendableTableEntry(extendableEntity, args);
                return;
            }

            var jsonEntry = args.Entity as JsonTableEntry;
            if (jsonEntry != null)
            {
                OnWritingEntityJsonTableEntry(jsonEntry, args);
            }
        }

        private void OnReadingEntity(object sender, ReadingWritingEntityEventArgs args)
        {
            var extendableEntity = args.Entity as ExtendableTableEntry;
            if (extendableEntity != null)
            {
                OnReadingExtendableTableEntry(extendableEntity, args);
                return;
            }

            var jsonEntry = args.Entity as JsonTableEntry;
            if (jsonEntry != null)
            {
                OnReadingJsonTableEntry(jsonEntry, args);
            }

        }

        static readonly XNamespace a = "http://www.w3.org/2005/Atom";
        static readonly XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        static readonly XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        private void OnWritingEntityExtendableTableEntry(ExtendableTableEntry extendableEntity, ReadingWritingEntityEventArgs args)
        {

            var properties = extendableEntity.GetType().GetProperties();//exclude all defined properties
            var propertiesEle = args.Data.Descendants(m + "properties").FirstOrDefault();
            if (propertiesEle == null)
                return;

            foreach (var val in extendableEntity.GetAllProperties()
                .Where(kv => properties.All(pp => pp.Name != kv.Key.Name)))    //also default save only saves changes doesn't replace
            {
                AddEdmVal(propertiesEle, val.Key.Name, val.Key.EdmTyp, val.Value);
            }
        }

        private void OnReadingExtendableTableEntry(ExtendableTableEntry extendableEntity, ReadingWritingEntityEventArgs args)
        {
            var properties = extendableEntity.GetType().GetProperties();//exclude all defined properties
            var propertiesEle = args.Data.Descendants(m + "properties").FirstOrDefault();
            if (propertiesEle == null)
                return;

            var q = from p in propertiesEle.Elements()
                    where properties.All(pp => pp.Name != p.Name.LocalName)
                    select new EdmRet(p);
        
            foreach (var dp in q)
            {
                extendableEntity.SetProperty(dp.Name, dp.Value, dp.TypeName);
            }
        }

        private void OnWritingEntityJsonTableEntry(JsonTableEntry jsonEntry, ReadingWritingEntityEventArgs args)
        {
            var propertiesEle = args.Data.Descendants(m + "properties").FirstOrDefault();
            if (propertiesEle == null)
                return;

            var listPart = SplitIntoChunks(jsonEntry.GetJson(), MaxStringSize);

            var edmTyp = Edm.String;
            for (int i = 0; i < listPart.Count; i++)
            {
                string col = "JsonPart" + i;
                AddEdmVal(propertiesEle, col, edmTyp, listPart[i]);
            }

            AddEdmVal(propertiesEle, "JsonCount", Edm.Int32, listPart.Count);
            AddEdmVal(propertiesEle, "JsonGzip", Edm.Boolean, false);
            AddEdmVal(propertiesEle, "JsonBinary", Edm.Boolean, false);
            
        }

        private void OnReadingJsonTableEntry(JsonTableEntry jsonEntry, ReadingWritingEntityEventArgs args)
        {
            var propertiesEle = args.Data.Descendants(m + "properties").FirstOrDefault();
            if (propertiesEle == null)
                return;

            var q = (from p in propertiesEle.Elements()
                     select new EdmRet(p)).ToDictionary(p => p.Name);
            
            if (!q.ContainsKey("JsonCount"))
                return;
            
            var jsonCount = q["JsonCount"].Get<int>();
            var jsonGzip = q["JsonGzip"].Get<bool>();
            var jsonBinary = q["JsonBinary"].Get<bool>();

            var res = Enumerable.Range(0, jsonCount)
                .Aggregate(new StringBuilder(), (s, i) => s.Append(q["JsonPart" + i].Value));

            jsonEntry.SetJson(res.ToString());

        }

        //        private const int MaxEntrySize = (int)((1024 * 1024) * 0.74);
        //        private const int MaxPropertySize = (int)((1024 * 64) * 0.74);
        private const int MaxStringSize = (1024 * 32);
        static List<string> SplitIntoChunks(string str, int chunkSize)
        {
            if(string.IsNullOrWhiteSpace(str))
                return new List<string>();

            return Enumerable.Range(0, (int)Math.Ceiling(str.Length / (double)chunkSize))
                .Select(
                    i => str.Substring(i * chunkSize
                                       , (i * chunkSize + chunkSize <= str.Length) ? chunkSize : str.Length - i * chunkSize))
                .ToList();
        }

        private static void AddEdmVal(XElement propertiesEle, string name, string edmTyp, object value)
        {
            var xElement = new XElement(d + name, Edm.ConvertToEdmValue(edmTyp, value));
            if (!Edm.IsString(edmTyp)) // don't add the string edm type. it's default
                xElement.SetAttributeValue(m + "type", edmTyp);
            if (value == null)
                xElement.SetAttributeValue(m + "null", true);
            propertiesEle.Add(xElement);
        }

        struct EdmRet
        {
            public EdmRet(XElement p)
            {
                Name = p.Name.LocalName;
                IsNull = string.Equals("true", p.Attribute(m + "null") == null ? null : p.Attribute(m + "null").Value,
                                       StringComparison.OrdinalIgnoreCase);
                TypeName = p.Attribute(m + "type") == null ? Edm.String : p.Attribute(m + "type").Value;
                Value = Edm.ConvertFromEdmValue(TypeName, p.Value, IsNull);
            }

            public readonly string Name;
            public readonly bool IsNull;
            public readonly string TypeName;
            public readonly object Value;

            public RetType Get<RetType>()
            {
                return (RetType)Value;
            }
        }
        
    }
}