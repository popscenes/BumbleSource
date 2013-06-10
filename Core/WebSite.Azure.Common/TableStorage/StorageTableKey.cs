using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

namespace Website.Azure.Common.TableStorage
{
    public class StorageTableKeyInterfaceComparer : IEqualityComparer<StorageTableKeyInterface>
    {
        public static readonly StorageTableKeyInterfaceComparer Instance = new StorageTableKeyInterfaceComparer();
        public bool Equals(StorageTableKeyInterface x, StorageTableKeyInterface y)
        {
            return x.PartitionKey == y.PartitionKey && x.RowKey == y.RowKey;
        }

        public int GetHashCode(StorageTableKeyInterface obj)
        {
            return (obj.RowKey ?? "").GetHashCode() ^ (obj.RowKey ?? "").GetHashCode();
        }
    }

    public interface StorageTableKeyInterface
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
    }

    public class StorageTableKey : TableServiceEntity, StorageTableKeyInterface
    {
    }
}