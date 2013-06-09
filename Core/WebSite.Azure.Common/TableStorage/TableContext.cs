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
using Website.Infrastructure.Util;
using Website.Infrastructure.Util.Extension;

namespace Website.Azure.Common.TableStorage
{
    public interface TableContextInterface
    {
        void InitTable<TableEntryType>(string tableName);

        IQueryable<TableEntryType> PerformQuery<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>> query = null, int take = -1);

        IQueryable<TableEntryType> PerformParallelQueries<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>>[] queries, int take = -1);

        IQueryable<SelectType> PerformSelectQuery<TableEntryType, SelectType>(string tableName, Expression<Func<TableEntryType, bool>> query, Expression<Func<TableEntryType, SelectType>> selectExpression, int take = -1);

        IQueryable<SelectType> PerformParallelSelectQueries<TableEntryType, SelectType>(string tableName, Expression<Func<TableEntryType, bool>>[] queries, Expression<Func<TableEntryType, SelectType>> selectExpression, int take = -1);

        DataServiceResponse SaveChanges();
        DataServiceResponse SaveChanges(SaveChangesOptions saveChangesOptions);
        void Delete<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>> query);
        void Delete(StorageTableKeyInterface tableEntry);
        void Store(string tableName, StorageTableKeyInterface tableEntry);
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
            if (AzureEnv.IsRunningInProdFabric())//no need in prod
                return;

            var cli = _account.CreateCloudTableClient();
            var cloudTable = cli.GetTableReference(tableName);

            if (cloudTable.Exists())
                return;

            Func<bool> create =
                () =>
                    {
                        cloudTable.CreateIfNotExists();
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

        public IQueryable<TableEntryType> PerformQuery<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>> query = null, int take = -1)
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

        public IQueryable<TableEntryType> PerformParallelQueries<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>>[] queries, int take = -1)
        {
            var res = new ConcurrentQueue<TableEntryType>();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(8, queries.Length) };
            Action<Expression<Func<TableEntryType, bool>>> parallelAction =
                (q) =>
                    {
                        var items = PerformQuery(tableName, query: q, take: take);
                        foreach (var tableEntry in items)
                        {
                            res.Enqueue(tableEntry);
                        }
                    };


            Parallel.ForEach(queries, parallelOptions, parallelAction);
            return res.AsQueryable();
        }

        public IQueryable<SelectType> PerformSelectQuery<TableEntryType, SelectType>(string tableName, Expression<Func<TableEntryType, bool>> query, Expression<Func<TableEntryType, SelectType>> selectExpression, int take = -1)
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

        public IQueryable<SelectType> PerformParallelSelectQueries<TableEntryType, SelectType>(string tableName, Expression<Func<TableEntryType, bool>>[] queries, Expression<Func<TableEntryType, SelectType>> selectExpression, int take = -1)
        {
            var res = new ConcurrentQueue<SelectType>();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(8, queries.Length) };
            Action<Expression<Func<TableEntryType, bool>>> parallelAction =
                (q) =>
                    {
                        var items = PerformSelectQuery(tableName, q, selectExpression, take: take);
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

        public void Delete<TableEntryType>(string tableName, Expression<Func<TableEntryType, bool>> query)
        {
            var entities = PerformQuery(tableName, query: query);
            foreach (var tableEntity in entities)
            {
                _containedContext.DeleteObject(tableEntity);
            }
        }

        public void Delete(StorageTableKeyInterface tableEntry)
        {
            if (_containedContext.GetEntityDescriptor(tableEntry) != null)
            {
                _containedContext.DeleteObject(tableEntry);
            }
        }

        public void Store(string tableName, StorageTableKeyInterface tableEntry)
        {
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

        private const string JsonPrefix = "Json";
        private const string JsonPart = "JsonPart";
        private const string JsonCount = "JsonCount";
        private const string JsonGzip = "JsonGzip";
        private const string JsonBinary = "JsonBinary";
        private const string JsonClrType = "JsonClrType";

        private static readonly ISet<string>
            ExcludeProps = typeof (JsonTableEntry).GetProperties().Select(p => p.Name)
                                                  .ToHashSet();

        private void OnWritingEntityJsonTableEntry(JsonTableEntry jsonEntry, ReadingWritingEntityEventArgs args)
        {
            var propertiesEle = args.Data.Descendants(m + "properties").FirstOrDefault();
            if (propertiesEle == null)
                return;

            var listPart = SplitIntoChunks(jsonEntry.GetJson(), Edm.MaxEdmStringSize);

            for (var i = 0; i < listPart.Count; i++)
            {
                var col = JsonPart + i;
                AddEdmVal(propertiesEle, col, Edm.String, listPart[i]);
            }

            AddEdmVal(propertiesEle, JsonCount, Edm.Int32, listPart.Count);
            AddEdmVal(propertiesEle, JsonGzip, Edm.Boolean, false);
            AddEdmVal(propertiesEle, JsonBinary, Edm.Boolean, false);
            AddEdmVal(propertiesEle, JsonClrType, Edm.String, jsonEntry.GetClrTyp());

            //just adds plain edm types from the aggregate root to the saved colums, handy for queries from linqpad or other sources
            if (jsonEntry.GetSourceObject() == null)
                return;

            var dict = jsonEntry.GetSourceObject().PropertiesToDictionary(null, ExcludeProps, false)
                .Where(pair => !pair.Key.StartsWith(JsonPrefix)).ToDictionary(pair => pair.Key, pair => pair.Value);
            var extendableEntity = ExtendableTableEntry.CreateFromDict(dict);
            foreach (var val in extendableEntity.GetAllProperties())
            {
                AddEdmVal(propertiesEle, val.Key.Name, val.Key.EdmTyp, val.Value);
            }
            
        }

        private void OnReadingJsonTableEntry(JsonTableEntry jsonEntry, ReadingWritingEntityEventArgs args)
        {
            var propertiesEle = args.Data.Descendants(m + "properties").FirstOrDefault();
            if (propertiesEle == null)
                return;

            var q = (from p in propertiesEle.Elements()
                     select new EdmRet(p)).ToDictionary(p => p.Name);
            
            if (!q.ContainsKey(JsonCount))
                return;

            var jsonCount = q[JsonCount].Get<int>();
            var jsonGzip = q[JsonGzip].Get<bool>();
            var jsonBinary = q[JsonBinary].Get<bool>();

            var res = Enumerable.Range(0, jsonCount)
                .Aggregate(new StringBuilder(), (s, i) => s.Append(q[JsonPart + i].Value));

            EdmRet clrTyp;
            var clrString = q.TryGetValue(JsonClrType, out clrTyp) ? clrTyp.Get<string>() : null;

            jsonEntry.SetJson(res.ToString(), clrString);

        }

        //        private const int MaxEntrySize = (int)((1024 * 1024) * 0.74);
        //        private const int MaxPropertySize = (int)((1024 * 64) * 0.74);
        
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
            try
            {

                var val = Edm.ConvertToEdmValue(edmTyp, value);

                if (!AzureEnv.UseRealStorage && Edm.IsString(edmTyp) && val is string)
                {
                    val = (val as String).Trim();//hack cause dev storage can't handle trailing spaces
                }

                var xElement = new XElement(d + name, val);
                if (!Edm.IsString(edmTyp)) // don't add the string edm type. it's default
                    xElement.SetAttributeValue(m + "type", edmTyp);
                else
                    xElement.SetAttributeValue(XNamespace.Xmlns + "space", "preserve");

                if (value == null)
                    xElement.SetAttributeValue(m + "null", true);
                propertiesEle.Add(xElement);
            }
            catch (NotSupportedException)
            {
                //skipping adding if edmType isn't supported or string to long etc
            }

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