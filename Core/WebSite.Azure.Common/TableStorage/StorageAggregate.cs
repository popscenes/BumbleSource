using System.Collections.Generic;
using System.Linq;

namespace Website.Azure.Common.TableStorage
{
    public class StorageAggregate
    {
        public class TableEntry<TableEntryType>
        {
            public string TableName { get; set; }
            public TableEntryType Entry { get; set; }
            public bool Deleted { get; set; }
        }

        private readonly TableNameAndPartitionProviderServiceInterface _nameAndPartitionProviderService;
        public StorageAggregate(object aggregateRoot, TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService)
        {
            _aggregateRoot = aggregateRoot;
            _nameAndPartitionProviderService = nameAndPartitionProviderService;
            _tableEntryCollection = new AggregateEntityTableEntryCollection();
        }

        //not needed atm, use if need to automatically set all cloned entries
//        public StorageAggregate(object aggregateRoot, Dictionary<object, ClonedTableEntry> tableEntries)
//        {
//            _aggregateRoot = aggregateRoot;
//            _tableEntryCollection = new AggregateEntityTableEntryCollection();
//            foreach (var clonedTableEntry in tableEntries)
//            {
//                _tableEntryCollection.SetEntry(clonedTableEntry.Key, clonedTableEntry.Value);
//            }
//        }

        private object _aggregateRoot;
        public object AggregateRoot
        {
            get { return _aggregateRoot; }
        }

        private AggregateEntityTableEntryCollection _tableEntryCollection;
        public AggregateEntityTableEntryCollection TableEntryCollection
        {
            get { return _tableEntryCollection; }
        }

        public void LoadAllTableEntriesForUpdate<TableEntryType>(TableContextInterface tableContext) where TableEntryType : class, StorageTableEntryInterface, new()
        {
            _tableEntryCollection.UpdateFrom(_aggregateRoot, _nameAndPartitionProviderService);
            foreach (var clonedTable in _tableEntryCollection.ClonedTableEntries)
            {
                var entity = clonedTable.Entity;
                var storage = clonedTable.TableEntry;
                storage.LoadFrom<TableEntryType>(tableContext, entity);
            }
        }

        public List<TableEntry<TableEntryType>> GetTableEntries<TableEntryType>(TableContextInterface tableContext) where TableEntryType : class, StorageTableEntryInterface, new()
        {
            _tableEntryCollection.UpdateFrom(_aggregateRoot, _nameAndPartitionProviderService);

            var ret = new List<TableEntry<TableEntryType>>();

            foreach (var clonedTable in _tableEntryCollection.ClonedTableEntries)
            {
                var entity = clonedTable.Entity;
                var storage = clonedTable.TableEntry;
                storage.PopulatePartitionClones<TableEntryType>(entity, tableContext);
                ret.AddRange(
                    storage.Entries.Select(
                        partition
                        => new TableEntry<TableEntryType>()
                               {
                                   Deleted = !clonedTable.IsActive,
                                   Entry = (TableEntryType)partition.Value,
                                   TableName = _nameAndPartitionProviderService.GetTableName(entity.GetType(), partition.Key)
                               }
                        )
                    );
            }
            return ret;
        }
    }
}