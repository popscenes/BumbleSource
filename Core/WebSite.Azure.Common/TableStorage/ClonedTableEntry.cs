using System.Collections.Generic;
using System.Linq;
using WebSite.Infrastructure.Domain;

namespace WebSite.Azure.Common.TableStorage
{    
    public class ClonedTableEntry<TableEntryType, DomainEntityType>
            where TableEntryType : class, StorageTableEntryInterface<DomainEntityType>, new()
    {
        private readonly Dictionary<int, TableEntryType> _entities;
        private readonly TableNameAndPartitionProviderInterface<DomainEntityType> _tableNameAndPartition;
        public ClonedTableEntry(TableNameAndPartitionProviderInterface<DomainEntityType> tableNameAndPartition)
        {
            _tableNameAndPartition = tableNameAndPartition;
            _entities = new Dictionary<int, TableEntryType>();
            CreateDefaultEntries(true);
        }

        public void SetPartitionEntity(int partition, TableEntryType tableEntry)
        {
            _entities[partition] = tableEntry;
        }

        public void CreateDefaultEntries(bool initToNull = false)
        {
            foreach (var partition in _tableNameAndPartition.GetPartitionIdentifiers(typeof(TableEntryType)))
            {
                _entities[partition] = !initToNull ? new TableEntryType() : null;
            }
        }

        public IEnumerable<StorageTableEntryInterface<DomainEntityType>> GetStorageTableEntries()
        {
            return _entities.Select(kv => kv.Value);
        }

        public void PopulatePartitionClones<ReadonlyDomainEntityType>(DomainEntityType entity, AzureTableContext tableContext) 
            where ReadonlyDomainEntityType : DomainEntityType, new()
        {
            LoadAllEntries<ReadonlyDomainEntityType>(tableContext, entity);

            UpdateEntries(entity);
        }

        private void LoadAllEntries<ReadonlyDomainEntityType>(AzureTableContext tableContext, DomainEntityType entity)
            where ReadonlyDomainEntityType : DomainEntityType, new()
        {
            //this means all partitions are loaded
            if (!_entities.Any(e => e.Value == null)) return;
            
            //create an entity that represents the current state.
            DomainEntityType currentState = entity;
            var entry = _entities.FirstOrDefault(e => e.Value != null);
            if(!entry.Equals(default(KeyValuePair<int, TableEntryType>)))
            {
                var source = entry.Value;
                currentState = new ReadonlyDomainEntityType();
                source.UpdateEntity(currentState);
            }


            foreach (var partition in _tableNameAndPartition.GetPartitionIdentifiers(typeof (TableEntryType)))
            {
                var partitionClone = _entities[partition];
                if (partitionClone == null) //attempt to load using the current state of the table entity
                {
                    var partKeyFunc = _tableNameAndPartition.GetPartitionKeyFunc(typeof (TableEntryType),
                                                                                 partition);
                    var rowKeyFunc = _tableNameAndPartition.GetRowKeyFunc(typeof (TableEntryType), partition);

                    var partitionKey = partKeyFunc(currentState);
                    var rowkey = rowKeyFunc(currentState);
                    partitionClone = tableContext.PerformQuery<TableEntryType>
                        (te => te.PartitionKey == partitionKey
                               && te.RowKey == rowkey, partition).SingleOrDefault();

                    _entities[partition] = partitionClone ?? new TableEntryType();
                }
            }
        }

        private void UpdateEntries(DomainEntityType entity)
        {
//update all table entities using the current domain entity state
            foreach (var partition in _tableNameAndPartition.GetPartitionIdentifiers(typeof (TableEntryType)))
            {
                var partKeyFunc = _tableNameAndPartition.GetPartitionKeyFunc(typeof (TableEntryType),
                                                                             partition);
                var rowKeyFunc = _tableNameAndPartition.GetRowKeyFunc(typeof (TableEntryType), partition);
                var partitionClone = _entities[partition];
                if (partitionClone == null) continue;

                var rowKey = rowKeyFunc(entity);
                var partitionKey = partKeyFunc(entity);
                partitionClone.KeyChanged = (!string.IsNullOrWhiteSpace(partitionClone.RowKey) &&
                                             (partitionClone.RowKey != rowKey ||
                                              partitionClone.PartitionKey != partitionKey));
                partitionClone.PartitionClone = partition;
                partitionClone.Update(entity);
                partitionClone.RowKey = rowKey;
                partitionClone.PartitionKey = partitionKey;
                _entities[partition] = partitionClone;
            }
        }
    }
}