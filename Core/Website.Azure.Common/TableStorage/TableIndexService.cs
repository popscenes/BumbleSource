using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Website.Azure.Common.DataServices;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Azure.Common.TableStorage
{
    public interface TableIndexServiceInterface
    {
        List<StorageType> FindEntitiesByIndex<EntityType, StorageType>(string indexName, string indexValue, int take = -1)
            where StorageType : StorageTableKeyInterface;

        void UpdateEntityIndexes<EntityType>(EntityType entity, bool deleteOnly = false) where EntityType : EntityIdInterface;
    }

    public static class StandardIndexSelectors
    {
        public const string FriendlyIdIndex = "FriendlyId";
        
        public static Expression<Func<EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>>
            FriendlyIdSelector<EntityInterfaceType>() where EntityInterfaceType : AggregateRootInterface
        {
            Expression<Func<EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory = root => new List<StorageTableKeyInterface>()
                {
                    new StorageTableKey()
                        {
                            PartitionKey = root.FriendlyId.ToStorageKeySection(),
                            RowKey = root.Id.ToStorageKeySection()
                        }
                };
            return indexEntryFactory;
        }
    }

    public class TableIndexService : TableIndexServiceInterface
    {
        private readonly TableNameAndIndexProviderServiceInterface _indexProviderService;
        private readonly TableContextInterface _tableContext;

        public TableIndexService(TableNameAndIndexProviderServiceInterface indexProviderService
            , TableContextInterface tableContext)
        {
            _indexProviderService = indexProviderService;
            _tableContext = tableContext;
        }

        public List<StorageType> FindEntitiesByIndex<EntityType, StorageType>(string indexName, string indexValue, int take = -1)
            where StorageType : StorageTableKeyInterface
        {
            var tableName = _indexProviderService.GetTableNameForIndex<EntityType>(indexName);
            var ret = _tableContext.PerformQuery<StorageType>(tableName
                , storage =>
                    storage.PartitionKey ==
                    (indexName.ToStorageKeySection() +
                    indexValue.ToStorageKeySection()));
            return ret.ToList();
        }

        public void UpdateEntityIndexes<EntityType>(EntityType entity, bool deleteOnly = false) where EntityType : EntityIdInterface
        {
            var indexes = _indexProviderService.GetAllIndexNamesFor<EntityType>().ToList();
            foreach (var index in indexes)
            {
                var lowerKey = index.ToStorageKeySection();
                var upperKey = index.ToStorageKeySection().GetValueForStartsWith();
                var rowKey =
                    _indexProviderService.GetIndexEntryFactoryFor<EntityType>(index)(entity)
                        .Select(e => e.RowKey)
                        .Distinct();


                var tableName = _indexProviderService.GetTableNameForIndex<EntityType>(index);
                foreach (var row in rowKey)
                {
                    _tableContext.Delete<StorageTableKey>(tableName, entry =>
                        entry.PartitionKey.CompareTo(lowerKey) >= 0 &&
                        entry.PartitionKey.CompareTo(upperKey) < 0 &&
                        entry.RowKey.Equals(row));
                }

            }

            _tableContext.SaveChangesRetryOnException();
            if (deleteOnly)
                return;

            var entries = indexes.Select(indexName => 
                new{ 
                        tableName = _indexProviderService.GetTableNameForIndex<EntityType>(indexName),
                        indexName,
                        entries = _indexProviderService.GetIndexEntryFactoryFor<EntityType>(indexName)(entity).Distinct(StorageTableKeyInterfaceComparer.Instance)
                }).ToList();

            foreach (var tabEntry in 
                from storageTableEntry in entries
                let tableName = storageTableEntry.tableName
                let indexName = storageTableEntry.indexName
                from entry in storageTableEntry.entries
                where entry.PartitionKey != null
                select new { tableName, entry, indexName })
            {
                tabEntry.entry.PartitionKey = tabEntry.indexName.ToStorageKeySection() + tabEntry.entry.PartitionKey;
                _tableContext.Store(tabEntry.tableName, tabEntry.entry);
            }

            _tableContext.SaveChangesRetryOnException();
        }

    }
}
