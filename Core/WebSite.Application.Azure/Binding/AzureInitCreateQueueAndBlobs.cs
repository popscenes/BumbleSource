using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Ninject;
using Ninject.Syntax;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Util;

namespace Website.Application.Azure.Binding
{
    public class AzureInitCreateQueueAndBlobs : InitServiceInterface
    {
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly CloudQueueClient _cloudQueueClient;
        private readonly CloudTableClient _cloudTableClient;

        public AzureInitCreateQueueAndBlobs(CloudBlobClient cloudBlobClient, CloudQueueClient cloudQueueClient,
            CloudTableClient cloudTableClient)
        {
            _cloudBlobClient = cloudBlobClient;
            _cloudQueueClient = cloudQueueClient;
            _cloudTableClient = cloudTableClient;
        }

        public void Init(IResolutionRoot iocContainer)
        {
            var imageContainer = _cloudBlobClient.GetContainerReference("imagestorage");
            imageContainer.CreateIfNotExists();
            var permissions = imageContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            imageContainer.SetPermissions(permissions);

            var appStorageContainer = _cloudBlobClient.GetContainerReference("applicationstorage");
            appStorageContainer.CreateIfNotExists();


//            iocContainer.Get<AzureTableContext>("broadcastCommunicators").InitFirstTimeUse();
//            iocContainer.Get<AzureTableContext>("websiteinfo").InitFirstTimeUse();

        }
    }
}
