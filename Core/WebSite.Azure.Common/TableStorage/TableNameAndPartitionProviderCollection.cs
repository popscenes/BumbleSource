using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WebSite.Azure.Common.TableStorage
{
    public class TableNameAndPartitionProviderCollection : TableNameAndPartitionProviderInterface, IEnumerable
    {
        private readonly List<TableNameAndPartitionProviderInterface> _nameAndPartitionProviderList
            = new List<TableNameAndPartitionProviderInterface>();

        public void Add(TableNameAndPartitionProviderInterface nameAndPartitionProvider)
        {
            _nameAndPartitionProviderList.Add(nameAndPartitionProvider);
        }

        public IEnumerable<KeyValuePair<Type, string>> GetAllTableTypesAndNames()
        {
            var tableTypesAndNames = new List<KeyValuePair<Type, string>>();
            foreach (var prov in _nameAndPartitionProviderList)
            {
                tableTypesAndNames.AddRange(prov.GetAllTableTypesAndNames());
            }
            return tableTypesAndNames;
        }

        public string GetTableName(Type type, int partition)
        {
            return _nameAndPartitionProviderList
                .Select(prov => prov.GetTableName(type, partition))
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
        }

        public int GetPartitionCount(Type type)
        {
            return _nameAndPartitionProviderList
                .Select(prov => prov.GetPartitionCount(type))
                .FirstOrDefault(count => count > 0);
        }

        public int[] GetPartitionIdentifiers(Type type)
        {
            return _nameAndPartitionProviderList
                .Aggregate(new List<int>(), 
                (l, v) =>
                        {
                            l.AddRange(v.GetPartitionIdentifiers(type));
                            return l;
                        }).ToArray();
        }

        public IEnumerator GetEnumerator()
        {
            return _nameAndPartitionProviderList.GetEnumerator();
        }
    }
}