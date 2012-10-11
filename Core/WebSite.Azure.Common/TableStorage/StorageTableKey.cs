using Microsoft.WindowsAzure.StorageClient;

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