using System;
using System.Collections.Generic;
using System.Linq;
using Website.Infrastructure.Domain;

namespace Website.Azure.Common.TableStorage
{

    public class ClonedTableEntry
    {
        private readonly Dictionary<int, StorageTableEntryInterface> _entities = new Dictionary<int, StorageTableEntryInterface>();
        private readonly TableNameAndPartitionProviderServiceInterface _nameAndPartitionProviderService;
        public ClonedTableEntry(TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService)
        {
            _nameAndPartitionProviderService = nameAndPartitionProviderService;
        }

        public void SetPartitionEntity<TableEntryType>(int partition, TableEntryType tableEntry)
            where TableEntryType : StorageTableEntryInterface
        {
            _entities[partition] = tableEntry;
        }

        public Dictionary<int, StorageTableEntryInterface> Entries
        {
            get { return _entities; }
        }

        public void PopulatePartitionClones<TableEntryType>(object entity, TableContextInterface tableContext)
            where TableEntryType : class, StorageTableEntryInterface, new()
        {
            LoadFrom<TableEntryType>(tableContext, entity);

            UpdateEntries<TableEntryType>(entity);
        }

        public void LoadFrom<TableEntryType>(TableContextInterface tableContext, object entity)
            where TableEntryType : class, StorageTableEntryInterface, new()
        {
            var entityTyp = entity.GetType();

            //uninitialized
            if (_entities.Count == 0)
                CreateDefaultEntries(entityTyp);
            //this means all partitions are loaded
            if (_entities.All(e => e.Value != null))
                return;

            //create an entity that represents the current state.
            object currentState = entity;
            var entry = _entities.FirstOrDefault(e => e.Value != null);
            if (!entry.Equals(default(KeyValuePair<int, StorageTableEntryInterface>)))
            {
                //if already loaded use this as the current state
                var source = entry.Value;
                if (source != null)
                    currentState = source.GetEntity(entityTyp);
            }

            foreach (var partition in _nameAndPartitionProviderService.GetPartitionIdentifiers(entityTyp))
            {
                var partitionClone = _entities[partition];
                if (partitionClone == null) //attempt to load using the current state of the table entity
                {
                    var partKeyFunc = _nameAndPartitionProviderService.GetPartitionKeyFunc(entityTyp, partition);
                    var rowKeyFunc = _nameAndPartitionProviderService.GetRowKeyFunc(entityTyp, partition);
                    var tableName = _nameAndPartitionProviderService.GetTableName(entityTyp, partition);

                    var partitionKey = partKeyFunc(currentState);
                    var rowkey = rowKeyFunc(currentState);
                    partitionClone = tableContext.PerformQuery<TableEntryType>
                        (tableName, te => te.PartitionKey == partitionKey
                               && te.RowKey == rowkey, partition).SingleOrDefault();

                    _entities[partition] = partitionClone ?? new TableEntryType();
                }
            }

        }

        private void CreateDefaultEntries(Type forEntityTyp)
        {
            foreach (var partition in _nameAndPartitionProviderService.GetPartitionIdentifiers(forEntityTyp))
            {
                _entities[partition] = null;
            }
        }

        private void UpdateEntries<TableEntryType>(object entity)
            where TableEntryType : class, StorageTableEntryInterface
        {
            var entityTyp = entity.GetType();
            //update all table entities using the current domain entity state
            foreach (var partition in _nameAndPartitionProviderService.GetPartitionIdentifiers(entityTyp))
            {
                var partKeyFunc = _nameAndPartitionProviderService.GetPartitionKeyFunc(entityTyp, partition);
                var rowKeyFunc = _nameAndPartitionProviderService.GetRowKeyFunc(entityTyp, partition);
                var partitionClone = _entities[partition] as TableEntryType;
                if (partitionClone == null) continue;

                var rowKey = rowKeyFunc(entity);
                var partitionKey = partKeyFunc(entity);
                partitionClone.KeyChanged = (!string.IsNullOrWhiteSpace(partitionClone.RowKey) &&
                                             (partitionClone.RowKey != rowKey ||
                                              partitionClone.PartitionKey != partitionKey));
                partitionClone.PartitionClone = partition;
                partitionClone.UpdateEntry(entity);
                partitionClone.RowKey = rowKey;
                partitionClone.PartitionKey = partitionKey;
                _entities[partition] = partitionClone;
            }
        }
    }
}