using Microsoft.WindowsAzure.StorageClient;
using Website.Application.Azure.Content;
using Website.Application.Command;
using Website.Infrastructure.Command;

namespace Website.Application.Azure.Command
{
    public class AzureCommandQueueFactory : CommandQueueFactoryInterface
    {
        private readonly CloudQueueClient _cloudQueueClient;
        private readonly CommandHandlerRespositoryInterface _handlerRespository;
        private readonly CloudBlobClient _cloudBlobClient;


        public AzureCommandQueueFactory(
            CloudBlobClient cloudBlobClient
            , CloudQueueClient cloudQueueClient
            , CommandHandlerRespositoryInterface handlerRespository)
        {
            _cloudBlobClient = cloudBlobClient;
            _cloudQueueClient = cloudQueueClient;
            _handlerRespository = handlerRespository;
        }

        public CommandBusInterface GetCommandBusForEndpoint(string queueEndpoint)
        {
            var queue = GetQueueForEndpoint(queueEndpoint);
            var commandSerializer = GetCommandSerializerForEndpoint(queueEndpoint);
            var messageFactory = new AzureMessageFactory();

            return new QueuedCommandBus(commandSerializer, queue, messageFactory);
        }

        public void Delete(string queueEndpoint)
        {
            var azureCloudQueue = new AzureCloudQueue(_cloudQueueClient.GetQueueReference(queueEndpoint));       
            if(azureCloudQueue.Exists())
                azureCloudQueue.Delete();

            var azureQueueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(queueEndpoint));
            if (azureQueueStorage.Exists())
                azureQueueStorage.Delete();
        }

        public QueuedCommandProcessor GetSchedulerForEndpoint(string queueEndpoint)
        {
            var queue = GetQueueForEndpoint(queueEndpoint);
            var commandSerializer = GetCommandSerializerForEndpoint(queueEndpoint);
            return new QueuedCommandProcessor(queue, commandSerializer, _handlerRespository);
        }

        private QueueInterface GetQueueForEndpoint(string queueEndpoint)
        {
            var azureCloudQueue = new AzureCloudQueue(_cloudQueueClient.GetQueueReference(queueEndpoint));  
            azureCloudQueue.CreateIfNotExist();
            return azureCloudQueue;
        }

        private CommandSerializerInterface GetCommandSerializerForEndpoint(string queueEndpoint)
        {
            var queueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(queueEndpoint));
            queueStorage.CreateIfNotExists();
            return new DataBusCommandSerializer(queueStorage);
        }
    }
}