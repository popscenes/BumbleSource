using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WebSite.Azure.Common.TableStorage
{
    public class TableNameAndPartitionProvider<EntityType> : TableNameAndPartitionProviderInterface<EntityType>, IEnumerable
    {
        private class TypePartitionInfo
        {         
            public string TableName { get; set; }
            public Func<EntityType, string> PartFunc { get; set; }
            public Func<EntityType, string> RowFunc { get; set; }
        }

        private readonly Dictionary<Type, Dictionary<int, TypePartitionInfo>> _typeToTablePart
            = new Dictionary<Type, Dictionary<int, TypePartitionInfo>>();

        public void Add(Type type, int partition, string name
            , Func<EntityType, string> partitionKeyFunc, Func<EntityType, string> rowKeyFunc = null)
        {
            var partitions = GetPartitions(type);
            if(partitions == null)
            {
                partitions = new Dictionary<int, TypePartitionInfo>();
                _typeToTablePart.Add(type, partitions);
            }

            partitions.Add(partition, new TypePartitionInfo()
                                   {PartFunc = partitionKeyFunc, RowFunc = rowKeyFunc, TableName = name});

        }

        private Dictionary<int, TypePartitionInfo> GetPartitions(Type type)
        {
            Dictionary<int, TypePartitionInfo> partitions;
            _typeToTablePart.TryGetValue(type, out partitions);
            return partitions;
        }

        public IEnumerable<KeyValuePair<Type, string>> GetAllTableTypesAndNames()
        {
            var ret = from kv in _typeToTablePart
                      from tn in kv.Value
                      select new KeyValuePair<Type, string>(kv.Key, tn.Value.TableName);
            return ret.ToList();
        }

        public Func<EntityType, string> GetPartitionKeyFunc(Type type, int partition)
        {
            var partitions = GetPartitions(type);
            return partitions != null ? partitions[partition].PartFunc : null;
        }

        public Func<EntityType, string> GetRowKeyFunc(Type type, int partition)
        {
            var partitions = GetPartitions(type);
            return partitions != null ? partitions[partition].RowFunc : null;
        }

        public int GetPartitionCount(Type type)
        {
            return !_typeToTablePart.ContainsKey(type) ? 0 : _typeToTablePart[type].Count;
        }

        public int[] GetPartitionIdentifiers(Type type)
        {
            var partitions = GetPartitions(type);
            return partitions != null ? partitions.Keys.ToArray() : null;
        }

        public string GetTableName(Type type, int partition)
        {
            if (!_typeToTablePart.ContainsKey(type))
                return null;
            var arr = _typeToTablePart[type];
            return arr[partition].TableName;
        }

        public IEnumerator GetEnumerator()
        {
            return GetAllTableTypesAndNames().GetEnumerator();
        }
    }
}