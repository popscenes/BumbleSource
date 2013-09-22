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
        private readonly MessageHandlerRespositoryInterface _handlerRespository;
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly QueueFactoryInterface _queueFactory;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;


        public AzureMessageQueueFactory(
            CloudBlobClient cloudBlobClient
            , QueueFactoryInterface queueFactory
            , MessageHandlerRespositoryInterface handlerRespository
            , UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _cloudBlobClient = cloudBlobClient;
            _handlerRespository = handlerRespository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _queueFactory = queueFactory;
        }

        public MessageBusInterface GetMessageBusForEndpoint(string queueEndpoint)
        {
            var queue = GetQueueForEndpoint(queueEndpoint);
            var commandSerializer = GetCommandSerializerForEndpoint(queueEndpoint);

            return new QueuedMessageBus(commandSerializer, queue);
        }

        public void Delete(string queueEndpoint)
        {
            _queueFactory.DeleteQueue(queueEndpoint);

            var azureQueueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(queueEndpoint));
            if (azureQueueStorage.Exists())
                azureQueueStorage.Delete();
        }

        public QueuedMessageProcessor GetProcessorForEndpoint(string queueEndpoint)
        {
            var queue = GetQueueForEndpoint(queueEndpoint);
            var commandSerializer = GetCommandSerializerForEndpoint(queueEndpoint);
            return new QueuedMessageProcessor(queue, commandSerializer, _handlerRespository, _unitOfWorkFactory);
        }

        private QueueInterface GetQueueForEndpoint(string queueEndpoint)
        {
            return _queueFactory.GetQueue(queueEndpoint);
        }

        private MessageSerializerInterface GetCommandSerializerForEndpoint(string queueEndpoint)
        {
            var queueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(queueEndpoint));
            queueStorage.CreateIfNotExists();
            return new DataBusMessageSerializer(queueStorage);
        }
    }
}