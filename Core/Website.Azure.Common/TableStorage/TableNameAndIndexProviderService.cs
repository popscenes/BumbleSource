using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Website.Azure.Common.TableStorage
{
    public class TableNameAndIndexProviderService : TableNameAndIndexProviderServiceInterface
    {
        
        protected class EntryBase
        {
            public Type EntityTyp { get; set; }
            public string TableName { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                var other = obj as TableEntry;
                return Equals(other);
            }

            public bool Equals(TableEntry other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return other.EntityTyp == EntityTyp;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var result = (EntityTyp != null ? EntityTyp.GetHashCode() : 0);
                    //result = (result * 397) ^ PartitionId;
                    return result;
                }
            }
        }
        protected class TableEntry : EntryBase
        {

            public Func<object, string> PartitionKeyFunc { get; set; }
            public Func<object, string> RowKeyFunc { get; set; }
        
        }

        protected class IndexEntry : EntryBase
        {
            public string IndexName { get; set; }
            public Func<object, IEnumerable<StorageTableKeyInterface>> IndexEntryFactory { get; set; }

        }

        protected class IndexEntry<EntityType> : IndexEntry
        {
            public Func<EntityType, IEnumerable<StorageTableKeyInterface>> IndexEntryFactoryTyp { get; set; }
        }

        protected class TableEntry<EntityType> : TableEntry
        {
            public Func<EntityType, string> PartitionKeyFuncTyp { get; set; }
            public Func<EntityType, string> RowKeyFuncTyp { get; set; }
        }

        private IEnumerable<EntryBase> AllEntries
        {
            get
            {
                return (new List<IEnumerable<EntryBase>>() { _entries, _indexEntries })
                    .SelectMany(l => l);
            }
        } 
        private readonly List<TableEntry> _entries = new List<TableEntry>();
        private readonly List<IndexEntry> _indexEntries = new List<IndexEntry>();
        

        public void Add<EntityType>(string tableName, Func<EntityType, string> partitionKeyFunc, Func<EntityType, string> rowKeyFunc = null) where EntityType : class
        {
            var entry = new TableEntry<EntityType>()
                            {
                                EntityTyp = typeof(EntityType),
                                PartitionKeyFuncTyp = partitionKeyFunc,
                                RowKeyFuncTyp = rowKeyFunc ?? partitionKeyFunc,
                                TableName = tableName
                            };

            entry.RowKeyFunc = o => entry.RowKeyFuncTyp(o as EntityType);
            entry.PartitionKeyFunc = o => entry.PartitionKeyFuncTyp(o as EntityType);
            if (_entries.Contains(entry))
                _entries.Remove(entry);

            //this creates a heirachy base <- subclass <- subclass
            InsertEntry(_entries, entry);
        }

        public void AddIndex<EntityQueryType, EntityIndexType>(string tableName, string indexname
            , Expression<Func<EntityIndexType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory)
                where EntityIndexType : class, EntityQueryType
        {

            var entry = new IndexEntry<EntityIndexType>()
                {
                    EntityTyp = typeof(EntityQueryType),
                    IndexEntryFactoryTyp = indexEntryFactory.Compile(),
                    TableName = tableName,
                    IndexName = indexname
                };
            entry.IndexEntryFactory = (o) => entry.IndexEntryFactoryTyp(o as EntityIndexType);
            InsertEntry(_indexEntries, entry);
        }

        private static void InsertEntry<EntryType>(List<EntryType> entries, EntryType entry) where EntryType : EntryBase
        {
            var firstIndex = entries.FindIndex(0, currentry => currentry.EntityTyp.IsAssignableFrom(entry.EntityTyp));
            if (firstIndex >= 0)
            {
                var indx = entries.FindIndex(firstIndex, currentry => !currentry.EntityTyp.IsAssignableFrom(entry.EntityTyp));
                if (indx >= 0)
                    entries.Insert(indx, entry);
                else
                    entries.Add(entry);
            }
            else
            {
                entries.Add(entry);
            }
        }



        public Func<object, string> GetPartitionKeyFunc<EntityType>()
        {
            return GetPartitionKeyFunc(typeof (EntityType));
        }

        public Func<object, string> GetRowKeyFunc<EntityType>()
        {
            return GetRowKeyFunc(typeof(EntityType));
        }

        public Func<object, string> GetPartitionKeyFunc(Type entityTyp)
        {
            var entry = GetEntry(entityTyp);
            return entry != null ? entry.PartitionKeyFunc : null;
        }

        public Func<object, string> GetRowKeyFunc(Type entityTyp)
        {
            var entry = GetEntry(entityTyp);
            return entry != null ? entry.RowKeyFunc : null;
        }

        public string GetTableName<EntityType>()
        {
            var entry = GetEntry<EntityType>();
            return entry != null ? entry.TableName : null;
        }

        public string GetTableName(Type entityTyp)
        {
            var entry = GetEntry(entityTyp);
            return entry != null ? entry.TableName : null;
        }

        public string GetTableNameForIndex<EntityType>(string indexname)
        {
            var entry = GetIndexEntry<EntityType>(indexname);
            return entry != null ? entry.TableName : null;
        }

        public string GetTableNameForIndex(Type entityTyp, string indexname)
        {
            var entry = GetIndexEntry(entityTyp, indexname);
            return entry != null ? entry.TableName : null;
        }

        public IEnumerable<string> GetAllIndexNamesFor<EntityType>()
        {
            return _indexEntries.Where(e => e.EntityTyp.IsAssignableFrom(typeof (EntityType)))
                                .Select(e => e.IndexName);

        }

        public Func<object, IEnumerable<StorageTableKeyInterface>> GetIndexEntryFactoryFor<EntityType>(string indexname)
        {
            var entry = GetIndexEntry<EntityType>(indexname);
            return entry != null ? entry.IndexEntryFactory : null;
        }

        private TableEntry GetEntry<EntityType>()
        {
            return GetEntry(typeof (EntityType));
        }

        private TableEntry GetEntry(Type entityTyp)
        {
            return _entries.LastOrDefault(e => e.EntityTyp.IsAssignableFrom(entityTyp));

        }

        private IndexEntry GetIndexEntry<EntityType>(string indexname)
        {
            return GetIndexEntry(typeof (EntityType), indexname);
        }

        private IndexEntry GetIndexEntry(Type entityTyp, string indexname)
        {
            return _indexEntries.LastOrDefault(e => e.EntityTyp.IsAssignableFrom(entityTyp)
                                               && e.IndexName == indexname);

        }


        
        public IEnumerable<string> GetAllTableNames()
        {
            var ret = new HashSet<string>();
            foreach (var entry in AllEntries.Where(entry => !ret.Contains(entry.TableName)))
            {
                ret.Add(entry.TableName);
            }

            return ret.ToList();
        }

        public void SuffixTableNames(string suffix)
        {
            foreach (var entry in AllEntries.Where(e => !e.TableName.StartsWith(suffix)))
            {
                entry.TableName = suffix + entry.TableName;
            }
        }
    }
}