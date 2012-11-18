using Microsoft.WindowsAzure.Storage.Table.DataServices;

namespace Website.Azure.Common.TableStorage
{
    public interface StorageTableKeyInterface
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
    }

    public class StorageTableKey : TableServiceEntity, StorageTableKeyInterface
    {
    }
}