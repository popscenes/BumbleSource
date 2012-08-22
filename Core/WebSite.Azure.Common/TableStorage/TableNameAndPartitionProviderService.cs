using System;
using System.Collections.Generic;
using System.Linq;

namespace WebSite.Azure.Common.TableStorage
{
    public class TableNameAndPartitionProviderService : TableNameAndPartitionProviderServiceInterface
    {
        protected class Entry
        {
            public Type EntityTyp { get; set; }
            public int PartitionId { get; set; }
            public string TableName { get; set; }
            public Func<object, string> PartitionKeyFunc { get; set; }
            public Func<object, string> RowKeyFunc { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                var other = obj as Entry;
                return Equals(other);
            }

            public bool Equals(Entry other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return other.EntityTyp == EntityTyp && other.PartitionId == PartitionId;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var result = (EntityTyp != null ? EntityTyp.GetHashCode() : 0);
                    result = (result*397) ^ PartitionId;
                    return result;
                }
            }
        }

        protected class Entry<EntityType> : Entry
        {
            public Func<EntityType, string> PartitionKeyFuncTyp { get; set; }
            public Func<EntityType, string> RowKeyFuncTyp { get; set; }
        }

        private readonly List<Entry> _entries = new List<Entry>();

        public void Add<EntityType>(int partitionId, string tableName, Func<EntityType, string> partitionKeyFunc, Func<EntityType, string> rowKeyFunc = null) where EntityType : class
        {
            var entry = new Entry<EntityType>()
                            {
                                EntityTyp = typeof(EntityType),
                                PartitionId = partitionId,
                                PartitionKeyFuncTyp = partitionKeyFunc,
                                RowKeyFuncTyp = rowKeyFunc ?? partitionKeyFunc,
                                TableName = tableName
                            };

            entry.RowKeyFunc = o => entry.RowKeyFuncTyp(o as EntityType);
            entry.PartitionKeyFunc = o => entry.PartitionKeyFuncTyp(o as EntityType);
            if (_entries.Contains(entry))
                _entries.Remove(entry);

            //this creates a heirachy base <- subclass <- subclass

            var firstIndex = _entries.FindIndex(0, currentry => currentry.EntityTyp.IsAssignableFrom(entry.EntityTyp));
            if(firstIndex >= 0)
            {
                var indx = _entries.FindIndex(firstIndex, currentry => !currentry.EntityTyp.IsAssignableFrom(entry.EntityTyp));
                if (indx >= 0)
                    _entries.Insert(indx, entry);
                else
                    _entries.Add(entry);
            }
            else
            {
                _entries.Add(entry);
            }

        }

        
        public int[] GetPartitionIdentifiers<EntityType>()
        {
            return GetPartitionIdentifiers(typeof (EntityType));
        }

        public int[] GetPartitionIdentifiers(Type entityTyp)
        {
            return _entries//.OfType<Entry<EntityType>>()//.Where(e => e.TableTyp == typeof(TableEntryType))
                .Where(e => e.EntityTyp.IsAssignableFrom(entityTyp))
                .Select(e => e.PartitionId).ToArray();
        }

        public Func<object, string> GetPartitionKeyFunc<EntityType>(int partition)
        {
            return GetPartitionKeyFunc(typeof (EntityType), partition);
        }

        public Func<object, string> GetRowKeyFunc<EntityType>(int partition)
        {
            return GetRowKeyFunc(typeof(EntityType), partition);
        }

        public Func<object, string> GetPartitionKeyFunc(Type entityTyp, int partition)
        {
            var entry = GetEntry(entityTyp, partition);
            return entry != null ? entry.PartitionKeyFunc : null;
        }

        public Func<object, string> GetRowKeyFunc(Type entityTyp, int partition)
        {
            var entry = GetEntry(entityTyp, partition);
            return entry != null ? entry.RowKeyFunc : null;
        }

        public string GetTableName<EntityType>(int partition)
        {
            var entry = GetEntry<EntityType>(partition);
            return entry != null ? entry.TableName : null;
        }

        public string GetTableName(Type entityTyp, int partition)
        {
            var entry = GetEntry(entityTyp, partition);
            return entry != null ? entry.TableName : null;
        }

        private Entry GetEntry<EntityType>(int partition)
        {
            return GetEntry(typeof (EntityType), partition);
        }

        private Entry GetEntry(Type entityTyp, int partition)
        {
            return _entries.LastOrDefault(e => e.EntityTyp.IsAssignableFrom(entityTyp)
                                               && e.PartitionId == partition);

        }

        
        public IEnumerable<string> GetAllTableNames()
        {
            var ret = new HashSet<string>();
            foreach (var entry in _entries.Where(entry => !ret.Contains(entry.TableName)))
            {
                ret.Add(entry.TableName);
            }
            return ret.ToList();
        }

        public void SuffixTableNames(string suffix)
        {
            foreach (var entry in _entries.Where(e => !e.TableName.StartsWith(suffix)))
            {
                entry.TableName = suffix + entry.TableName;
            }
        }
    }
}