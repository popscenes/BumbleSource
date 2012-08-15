using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using WebSite.Azure.Common.DataServices;
using WebSite.Azure.Common.Environment;

namespace WebSite.Azure.Common.TableStorage
{
    public class AzureTableContext 
    {
        private readonly CloudStorageAccount _account;
        private readonly TableNameAndPartitionProviderInterface _tableNameAndPartitionProvider;
        private readonly PropertyGroupTableSerializerInterface _propertyGroupTableSerializer;
        private readonly TableServiceContext _containedContext;

        public AzureTableContext(CloudStorageAccount account
            , TableNameAndPartitionProviderInterface tableNameAndPartitionProvider
            , PropertyGroupTableSerializerInterface propertyGroupTableSerializer)
        {
            _account = account;
            _tableNameAndPartitionProvider = tableNameAndPartitionProvider;
            _propertyGroupTableSerializer = propertyGroupTableSerializer;

            _containedContext = new TableServiceContext(account.TableEndpoint.AbsoluteUri, account.Credentials);
            _containedContext.WritingEntity += OnWritingEntity;
            _containedContext.ReadingEntity += OnReadingEntity;
        }

        public void InitFirstTimeUse()
        {           
            var cli = _account.CreateCloudTableClient();
            foreach (var entry in _tableNameAndPartitionProvider.GetAllTableTypesAndNames())
            {
                if (cli.DoesTableExist(entry.Value))
                    continue;

                Func<bool> create =
                    () =>
                        {
                            cli.CreateTable(entry.Value);
                            SaveChanges();
                            return true;
                        };
                DataServicesQueryHelper.QueryRetry(create);
                     
                var initOb =
                    Activator.CreateInstance(entry.Key, true) as TableServiceEntity;
                if (initOb == null)
                    throw new ArgumentException(
                        "Entity type must derive from TableServiceEntity");
                initOb.PartitionKey = "123";
                initOb.RowKey = "123";

                Func<bool> add =
                    () =>
                    {
                        _containedContext.AddObject(entry.Value, initOb);
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
                   
        }

        private DataServiceQuery<TableEntryType> QueryFor<TableEntryType>(int partition = 0)
        {
            return
                _containedContext.CreateQuery<TableEntryType>(_tableNameAndPartitionProvider.GetTableName(typeof(TableEntryType),
                                                                                              partition));
        }

        private IQueryable<TableEntryType> ExecuteQuery<TableEntryType>(IQueryable<TableEntryType> query, int take = -1)
        {
           var ret = query.AsTableServiceQuery();
            if(take > 0)
                ret = ret.Take(take).AsTableServiceQuery();
            
            return ret.Execute().ToList()//cause evaluation of enumeration to occur;
                .AsQueryable();
        }

        /// <summary>
        /// Using this method to limit the ways people can approach queries
        /// basically want to ensure that AsTableServiceQuery() is used etc
        /// to provide default retries and fetch next etc
        /// DataServicesQueryHelper.QueryRetry isn't needed for retries any more (AsTableServiceQuery() handles it)
        /// but will leave in case there is another reason to retry, it also does exception handling
        /// </summary>
        /// <typeparam name="TableEntryType"></typeparam>
        /// <param name="query"></param>
        /// <param name="partition"></param>
        /// <param name="take"> </param>
        /// <param name="skip"> </param>
        /// <returns></returns>
        public IQueryable<TableEntryType> PerformQuery<TableEntryType>(Expression<Func<TableEntryType, bool>> query = null
            , int partition = 0, int take = -1)
        {
            Func<IQueryable<TableEntryType>> exec = () =>
            {
                var execQuery =
                    QueryFor<TableEntryType>(partition) as
                    IQueryable<TableEntryType>;
               
                if (query != null) 
                    execQuery = execQuery.Where(query);

                return ExecuteQuery(execQuery, take);
            };

            var ret = DataServicesQueryHelper.QueryRetry(exec);
            return ret ?? (new List<TableEntryType>()).AsQueryable();
        }

        public IQueryable<TableEntryType> PerformParallelQueries<TableEntryType>(Expression<Func<TableEntryType, bool>>[] queries
            , int partition = 0, int take = -1)
        {
            var res= new ConcurrentQueue<TableEntryType>();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(8, queries.Length) };
            Action<Expression<Func<TableEntryType, bool>>> parallelAction =
                (q) =>
                    {
                        var items = PerformQuery(q, partition, take);
                        foreach (var tableEntry in items)
                        {
                            res.Enqueue(tableEntry);    
                        }                       
                    };

            
            Parallel.ForEach(queries, parallelOptions, parallelAction);
            return res.AsQueryable();
        }

        /// <summary>
        /// !!!!as at at 1.6 sdk Development storage doesn't fucking support projection queries
        ///  only wasted half a day on this..... this method will not work in devel atm
        ///
        /// Same as PerformQuery, however allows projection
        /// Use this for selecting just a subset of table properties
        /// </summary>
        /// <typeparam name="TableEntryType"></typeparam>
        /// <typeparam name="SelectType"></typeparam>
        /// <param name="query"></param>
        /// <param name="selectExpression"></param>
        /// <param name="partition"></param>
        ///<param name="take"> </param>
        ///<returns></returns>
        public IQueryable<SelectType> PerformSelectQuery<TableEntryType, SelectType>(Expression<Func<TableEntryType, bool>> query
            , Expression<Func<TableEntryType, SelectType>> selectExpression
            , int partition = 0, int take = -1)
        {
            //just fake it if not using real storage
            //throw new NotSupportedException("as at at 1.6 sdk Development storage doesn't fucking support projection queries");

            if(!AzureEnv.UseRealStorage)
            {
                Func<IQueryable<TableEntryType>> exec = () =>
                {
                    var execQuery =
                        QueryFor<TableEntryType>(partition) as
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
                        QueryFor<TableEntryType>(partition) as
                        IQueryable<TableEntryType>;
                    execQuery = execQuery.Where(query);
                    return ExecuteQuery(execQuery.Select(selectExpression));
                };

                var ret = DataServicesQueryHelper.QueryRetry(exec);
                return ret ?? (new List<SelectType>()).AsQueryable();
            }

        }

        public IQueryable<SelectType> PerformParallelSelectQueries<TableEntryType, SelectType>(Expression<Func<TableEntryType, bool>>[] queries
            , Expression<Func<TableEntryType, SelectType>> selectExpression
            , int partition = 0, int take = -1)
        {
            var res = new ConcurrentQueue<SelectType>();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(8, queries.Length) };
            Action<Expression<Func<TableEntryType, bool>>> parallelAction =
                (q) =>
                {
                    var items = PerformSelectQuery(q, selectExpression, partition, take);
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
            return _containedContext.SaveChangesWithRetries(SaveChangesOptions.ReplaceOnUpdate);
        }

        public DataServiceResponse SaveChanges(SaveChangesOptions saveChangesOptions)
        {
            return _containedContext.SaveChangesWithRetries(saveChangesOptions);
        }

        public void Delete<TableEntryType>(Expression<Func<TableEntryType, bool>> query, int partition = 0)
        {
            var entities = PerformQuery(query, partition);
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

        public void Store(StorageTableEntryInterface tableEntry)
        {
            Store(new List<StorageTableEntryInterface>(){tableEntry});
        }

        public void Store(IEnumerable<StorageTableEntryInterface> tableEntries)
        {
            var tableEntriesList = tableEntries.ToList();
            foreach (var tableServiceEntity in tableEntriesList.Where(ts => ts.KeyChanged))
            {                 
                _containedContext.DeleteObject(tableServiceEntity);
            }

            if (tableEntriesList.Any(ts => ts.KeyChanged))
                this.SaveChanges();

            foreach (var tableEntry in tableEntriesList)
            {
                //do insert or replace in prod env
                if (AzureEnv.UseRealStorage)
                {
                    if (_containedContext.GetEntityDescriptor(tableEntry) == null)
                    {
                        var tableName = _tableNameAndPartitionProvider.GetTableName(tableEntry.GetType(), tableEntry.PartitionClone);
                        _containedContext.AttachTo(tableName, tableEntry);
                    }

                    _containedContext.UpdateObject(tableEntry);
                }
                else
                {
                    if (_containedContext.GetEntityDescriptor(tableEntry) == null)
                    {
                        var tableName = _tableNameAndPartitionProvider.GetTableName(tableEntry.GetType(), tableEntry.PartitionClone);
                        _containedContext.AddObject(tableName, tableEntry);
                    }
                    else
                    {
                        _containedContext.UpdateObject(tableEntry);
                    }
                }
            }
        }

        private void WriteExtra(ExtendableTableEntry extendableEntry)
        {
            var propertyGroupCollection = extendableEntry as HasPropertyGroupCollectionInterface;
            if (propertyGroupCollection != null && propertyGroupCollection.GetPropertyGroupCollection() != null)
            {
                _propertyGroupTableSerializer.MergeProperties(extendableEntry, propertyGroupCollection.GetPropertyGroupCollection());
            }
        }

        private void ReadExtra(ExtendableTableEntry extendableEntry)
        {
            var propertyGroupCollection = extendableEntry as HasPropertyGroupCollectionInterface;
            if (propertyGroupCollection != null && propertyGroupCollection.GetPropertyGroupCollection() != null)
            {
                _propertyGroupTableSerializer.LoadProperties(propertyGroupCollection.GetPropertyGroupCollection(),
                                                             extendableEntry);
            }
        }

        private void OnWritingEntity(object sender, ReadingWritingEntityEventArgs args)
        {
            var extendableEntity = args.Entity as ExtendableTableEntry;
            if (extendableEntity == null)
                return;

            WriteExtra(extendableEntity);

            XNamespace a = "http://www.w3.org/2005/Atom";
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

            var properties = extendableEntity.GetType().GetProperties();//exclude all defined properties
            var propertiesEle = args.Data.Descendants(m + "properties").FirstOrDefault();
            if (propertiesEle == null)
                return;

            foreach (var val in extendableEntity.GetAllProperties()
                .Where(kv => properties.All(pp => pp.Name != kv.Key.Name)))    //also default save only saves changes doesn't replace
            {
                var xElement = new XElement(d + val.Key.Name, Edm.ConvertToEdmValue(val.Key.EdmTyp, val.Value));
                if (!Edm.IsString(val.Key.EdmTyp)) // don't add the string edm type. it's default
                    xElement.SetAttributeValue(m + "type", val.Key.EdmTyp);
                if (val.Value == null)
                    xElement.SetAttributeValue(m + "null", true);
                propertiesEle.Add(xElement);
            }
        }

        private void OnReadingEntity(object sender, ReadingWritingEntityEventArgs args)
        {
            var extendableEntity = args.Entity as ExtendableTableEntry;
            if (extendableEntity == null)
                return;

            XNamespace a = "http://www.w3.org/2005/Atom";
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";


            var properties = extendableEntity.GetType().GetProperties();//exclude all defined properties
            var propertiesEle = args.Data.Descendants(m + "properties").FirstOrDefault();
            if (propertiesEle == null)
                return;

            var q = from p in propertiesEle.Elements()
            where properties.All(pp => pp.Name != p.Name.LocalName)
            select new
            {
                Name = p.Name.LocalName,
                IsNull = string.Equals("true", p.Attribute(m + "null") == null ? null : p.Attribute(m + "null").Value, StringComparison.OrdinalIgnoreCase),
                TypeName = p.Attribute(m + "type") == null ? Edm.String : p.Attribute(m + "type").Value,
                p.Value
            };

            foreach (var dp in q)
            {
                extendableEntity.SetProperty(dp.Name
                    , Edm.ConvertFromEdmValue(dp.TypeName, dp.Value, dp.IsNull)
                    , dp.TypeName);
            }

            ReadExtra(extendableEntity);
        }
    }
}