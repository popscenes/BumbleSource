using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Website.Application.Azure.Content;
using Website.Application.Azure.Queue;
using Website.Application.Command;
using Website.Application.Queue;
using Website.Infrastructure.Command;

namespace Website.Application.Azure.Command
{
    public class AzureCommandQueueFactory : CommandQueueFactoryInterface
    {
        private readonly CloudQueueClient _cloudQueueClient;
        private readonly CommandHandlerRespositoryInterface _handlerRespository;
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly AzureCloudQueueFactory _azureCloudQueueFactory;


        public AzureCommandQueueFactory(
            CloudBlobClient cloudBlobClient
            , CloudQueueClient cloudQueueClient
            , CommandHandlerRespositoryInterface handlerRespository)
        {
            _cloudBlobClient = cloudBlobClient;
            _cloudQueueClient = cloudQueueClient;
            _handlerRespository = handlerRespository;
            _azureCloudQueueFactory = new AzureCloudQueueFactory(cloudQueueClient);
        }

        public CommandBusInterface GetCommandBusForEndpoint(string queueEndpoint)
        {
            var queue = GetQueueForEndpoint(queueEndpoint);
            var commandSerializer = GetCommandSerializerForEndpoint(queueEndpoint);

            return new QueuedCommandBus(commandSerializer, queue);
        }

        public void Delete(string queueEndpoint)
        {
            _azureCloudQueueFactory.DeleteQueue(queueEndpoint);

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
            return _azureCloudQueueFactory.GetQueue(queueEndpoint);
        }

        private CommandSerializerInterface GetCommandSerializerForEndpoint(string queueEndpoint)
        {
            var queueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(queueEndpoint));
            queueStorage.CreateIfNotExists();
            return new DataBusCommandSerializer(queueStorage);
        }
    }
}