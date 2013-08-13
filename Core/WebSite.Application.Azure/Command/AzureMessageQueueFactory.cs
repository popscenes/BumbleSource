using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Website.Application.Azure.Content;
using Website.Application.Azure.Queue;
using Website.Application.Messaging;
using Website.Application.Queue;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace Website.Application.Azure.Command
{
    public class AzureMessageQueueFactory : MessageQueueFactoryInterface
    {
        private readonly CloudQueueClient _cloudQueueClient;
        private readonly MessageHandlerRespositoryInterface _handlerRespository;
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly AzureCloudQueueFactory _azureCloudQueueFactory;


        public AzureMessageQueueFactory(
            CloudBlobClient cloudBlobClient
            , CloudQueueClient cloudQueueClient
            , MessageHandlerRespositoryInterface handlerRespository)
        {
            _cloudBlobClient = cloudBlobClient;
            _cloudQueueClient = cloudQueueClient;
            _handlerRespository = handlerRespository;
            _azureCloudQueueFactory = new AzureCloudQueueFactory(cloudQueueClient);
        }

        public MessageBusInterface GetMessageBusForEndpoint(string queueEndpoint)
        {
            var queue = GetQueueForEndpoint(queueEndpoint);
            var commandSerializer = GetCommandSerializerForEndpoint(queueEndpoint);

            return new QueuedMessageBus(commandSerializer, queue);
        }

        public void Delete(string queueEndpoint)
        {
            _azureCloudQueueFactory.DeleteQueue(queueEndpoint);

            var azureQueueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(queueEndpoint));
            if (azureQueueStorage.Exists())
                azureQueueStorage.Delete();
        }

        public QueuedMessageProcessor GetProcessorForEndpoint(string queueEndpoint)
        {
            var queue = GetQueueForEndpoint(queueEndpoint);
            var commandSerializer = GetCommandSerializerForEndpoint(queueEndpoint);
            return new QueuedMessageProcessor(queue, commandSerializer, _handlerRespository);
        }

        private QueueInterface GetQueueForEndpoint(string queueEndpoint)
        {
            return _azureCloudQueueFactory.GetQueue(queueEndpoint);
        }

        private MessageSerializerInterface GetCommandSerializerForEndpoint(string queueEndpoint)
        {
            var queueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(queueEndpoint));
            queueStorage.CreateIfNotExists();
            return new DataBusMessageSerializer(queueStorage);
        }
    }
}