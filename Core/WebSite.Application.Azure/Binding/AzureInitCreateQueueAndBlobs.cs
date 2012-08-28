using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using Ninject.Syntax;
using Website.Application.Command;
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
            imageContainer.CreateIfNotExist();
            var permissions = imageContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            imageContainer.SetPermissions(permissions);

//            iocContainer.Get<AzureTableContext>("broadcastCommunicators").InitFirstTimeUse();
//            iocContainer.Get<AzureTableContext>("websiteinfo").InitFirstTimeUse();

        }
    }
}
