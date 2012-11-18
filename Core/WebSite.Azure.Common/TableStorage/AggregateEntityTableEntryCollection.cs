using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Util;

namespace Website.Azure.Common.TableStorage
{
    public class AggregateEntityTableEntryCollection
    {
        public class Entry
        {
            public object Entity { get; set; }
            public ClonedTableEntry TableEntry { get; set; }
            public bool IsActive { get; set; }
        }

        private readonly List<Entry> _clonedTableEntries = new List<Entry>();
        public List<Entry> ClonedTableEntries
        {
            get { return _clonedTableEntries; }
        }

        public void SetEntry(object entity, ClonedTableEntry tableEntry)
        {
            _clonedTableEntries.RemoveAll(e => e.Entity.Equals(entity));
            _clonedTableEntries.Add(new Entry(){Entity = entity, TableEntry = tableEntry, IsActive = true});
        }

        public void UpdateFrom(object root, TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService)
        {
            var entities = new HashSet<object>(); 
            AggregateMemberEntityAttribute.GetAggregateEnities(entities, root);

            foreach (var clonedTableEntry in _clonedTableEntries)
            {
                clonedTableEntry.IsActive = false;
            }

            foreach (var entity in entities)
            {
                AddIfNotExists(entity, nameAndPartitionProviderService);
            }
        }

        private void AddIfNotExists(object entity, TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService)
        {
            var val = _clonedTableEntries.SingleOrDefault(e => e.Entity.Equals(entity));
            if (val != null)
                val.IsActive = true;
            else
                _clonedTableEntries.Add(new Entry(){Entity = entity, TableEntry = new ClonedTableEntry(nameAndPartitionProviderService)
                    , IsActive = true});
        }
    }
}