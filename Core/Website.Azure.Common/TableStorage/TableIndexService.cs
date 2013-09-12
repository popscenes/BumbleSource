using System;
using System.Collections.Concurrent;
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
using Website.Infrastructure.Query;

namespace Website.Azure.Common.TableStorage
{

    public interface TableIndexServiceInterface
    {
        List<StorageType> FindEntitiesByIndex<EntityType, StorageType>(Index<EntityType> index, string indexValue, bool encodeValue = true, int take = -1)
            where StorageType : StorageTableKeyInterface;

        List<StorageType> FindEntitiesByIndexPrefix<EntityType, StorageType>(Index<EntityType> index, IEnumerable<string> keyparts, bool encodeValue = true, int take = -1)
            where StorageType : StorageTableKeyInterface;

        List<StorageType> FindEntitiesByIndexRange<EntityType, StorageType>(Index<EntityType> index, string keymin, string keymax, bool encodeValue = true, int take = -1)
            where StorageType : StorageTableKeyInterface;

        List<StorageType> FindEntitiesByIndexRange<EntityType, StorageType>(Index<EntityType> index, string keymin, string keymax, string rowkeymin, string rowkeymax, bool encodeValue = true, int take = -1)
            where StorageType : StorageTableKeyInterface;

        void UpdateEntityIndexes<EntityType>(EntityType entity, bool deleteOnly = false) where EntityType : EntityIdInterface;
    }

    public class FriendlyIdIndexDefinition<EntityIndexType> : IndexDefinition<EntityIndexType, EntityIndexType> 
        where EntityIndexType : class, EntityIdInterface
    {
        public override Expression<Func<QueryChannelInterface, EntityIndexType, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, EntityIndexType, IEnumerable<StorageTableKeyInterface>>>
                    indexEntryFactory = (qc, root) => new List<StorageTableKeyInterface>()
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

        public override string IndexName
        {
            get { return "FriendlyId"; }
        }
    }

    public class TableIndexService : TableIndexServiceInterface
    {
        private readonly TableNameAndIndexProviderServiceInterface _indexProviderService;
        private readonly TableContextInterface _tableContext;
        private readonly QueryChannelInterface _queryChannel;

        public TableIndexService(TableNameAndIndexProviderServiceInterface indexProviderService
            , TableContextInterface tableContext, QueryChannelInterface queryChannel)
        {
            _indexProviderService = indexProviderService;
            _tableContext = tableContext;
            _queryChannel = queryChannel;
        }

        public List<StorageType> FindEntitiesByIndex<EntityType, StorageType>(Index<EntityType> index, string indexValue, bool encodeValue = true, int take = -1)
            where StorageType : StorageTableKeyInterface
        {
            var tableName = _indexProviderService.GetTableNameForIndex<EntityType>(index.IndexName);
            var valuePart = encodeValue ? indexValue.ToStorageKeySection() : indexValue;

            var ret = _tableContext.PerformQuery<StorageType>(tableName
                , storage =>
                    storage.PartitionKey ==
                    (index.IndexName.ToStorageKeySection() + valuePart));
            return ret.ToList();
        }

        public List<StorageType> FindEntitiesByIndexPrefix<EntityType, StorageType>(Index<EntityType> index, IEnumerable<string> keyparts, bool encodeValue = true, int take = -1) where StorageType : StorageTableKeyInterface
        {
            var tableName = _indexProviderService.GetTableNameForIndex<EntityType>(index.IndexName);

            var res = new ConcurrentQueue<StorageType>();
            Parallel.ForEach(keyparts, keypart =>
                {
                    var val = encodeValue ? keypart.ToStorageKeySection() : keypart;
                    var lowerKey = (index.IndexName.ToStorageKeySection() + val.TrimEnd(']'));

                    var upperKey = lowerKey.GetEndValueForStartsWith();

                    var ret = _tableContext.PerformQuery<StorageType>(tableName
                        , storage =>
                                storage.PartitionKey.CompareTo(lowerKey) >= 0 &&
                                storage.PartitionKey.CompareTo(upperKey) < 0);
                    foreach (var storage in ret)
                        res.Enqueue(storage);
                });

            var best = from g in
                       from s in res.ToList()
                       group s by s.RowKey.ExtractEntityIdFromRowKey()
                           orderby  g.Count() descending, g.First().PartitionKey ascending
                           select g.First();
            
            return best.ToList();
        }

        public List<StorageType> FindEntitiesByIndexRange<EntityType, StorageType>(Index<EntityType> index, string keymin, string keymax, bool encodeValue = true, int take = -1) where StorageType : StorageTableKeyInterface
        {
            var tableName = _indexProviderService.GetTableNameForIndex<EntityType>(index.IndexName);
            var lowerKey = index.IndexName.ToStorageKeySection() + (encodeValue ? keymin.ToStorageKeySection() : keymin);
            var highKey = index.IndexName.ToStorageKeySection() + (encodeValue ? keymax.ToStorageKeySection() : keymax);


            var ret = _tableContext.PerformQuery<StorageType>(tableName
                , storage =>
                        storage.PartitionKey.CompareTo(lowerKey) >= 0 &&
                        storage.PartitionKey.CompareTo(highKey) <= 0);
            
            return ret.ToList();
        }

        public List<StorageType> FindEntitiesByIndexRange<EntityType, StorageType>(Index<EntityType> index, string keymin, string keymax, string rowkeymin, string rowkeymax, bool encodeValue = true, int take = -1) where StorageType : StorageTableKeyInterface
        {
            var tableName = _indexProviderService.GetTableNameForIndex<EntityType>(index.IndexName);
            var lowerKey = index.IndexName.ToStorageKeySection() + (encodeValue ? keymin.ToStorageKeySection() : keymin);
            var highKey = index.IndexName.ToStorageKeySection() + (encodeValue ? keymax.ToStorageKeySection() : keymax);

            var lowerRowKey = (encodeValue ? rowkeymin.ToStorageKeySection() : rowkeymin);
            var highRowKey = (encodeValue ? rowkeymax.ToStorageKeySection() : rowkeymax);


            var ret = _tableContext.PerformQuery<StorageType>(tableName
                , storage =>
                        storage.PartitionKey.CompareTo(lowerKey) >= 0 &&
                        storage.PartitionKey.CompareTo(highKey) <= 0 &&
                        storage.RowKey.CompareTo(lowerRowKey) >= 0 &&
                        storage.RowKey.CompareTo(highRowKey) <=0);

            return ret.ToList();
        }

        public void UpdateEntityIndexes<EntityType>(EntityType entity, bool deleteOnly = false) where EntityType : EntityIdInterface
        {
            var indexes = _indexProviderService.GetAllIndexNamesForUpdate<EntityType>().ToList();
            foreach (var index in indexes)
            {
                var lowerKey = index.ToStorageKeySection();
                var upperKey = index.ToStorageKeySection().GetEndValueForStartsWith();
                var rowKey =
                    _indexProviderService.GetIndexEntryFactoryFor<EntityType>(index)(_queryChannel, entity)
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
                        entries = _indexProviderService.GetIndexEntryFactoryFor<EntityType>(indexName)(_queryChannel, entity).Distinct(StorageTableKeyInterfaceComparer.Instance)
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
