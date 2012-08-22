using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Util;

namespace WebSite.Azure.Common.TableStorage
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
            GetAggregateEnities(entities, root);

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

        private void GetAggregateEnities(HashSet<object> list, object root)
        {
            if(list.Contains(root))
            {
                Trace.TraceWarning("Aggregate list already contains object, possible circular reference in aggregate: " + root);
                return;
            }
            
            list.Add(root);

            var props = SerializeUtil.GetPropertiesWithAttribute(root.GetType(), typeof (AggregateMemberEntityAttribute));
            var topLevelMembers = new List<object>();
            foreach (var propertyInfo in props)
            {
                if(propertyInfo.PropertyType.FindInterfaces((m, criteria) => 
                                                            m.Equals(criteria), typeof(IEnumerable)).Any())
                {
                    var coll = propertyInfo.GetValue(root, null) as IEnumerable;
                    topLevelMembers.AddRange(coll.Cast<object>());
                }
                else if(!propertyInfo.GetIndexParameters().Any())
                {
                    topLevelMembers.Add(propertyInfo.GetValue(root, null));
                }
                else
                {
                    throw new ArgumentException("Can't apply AggregateMemberEntityAttribute to index parameters atm: " + propertyInfo.Name);
                }
            }

            foreach (var topLevelMember in topLevelMembers)
            {
                GetAggregateEnities(list, topLevelMember);
            }

        }
    }
}