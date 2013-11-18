using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Website.Application.Azure.Content;
using Website.Application.Azure.Queue;
using Website.Application.Azure.ServiceBus;
using Website.Application.Messaging;
using Website.Application.Queue;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace Website.Application.Azure.Command
{
    public class AzureMessageQueueFactory : MessageQueueFactoryInterface
    {
        
        private readonly QueueProcessorTask _queueProcessorTask;
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly QueueFactoryInterface _queueFactory;

        private readonly EventTopicSenderFactoryInterface _eventTopicSenderFactory;
        private readonly EventSubscriptionRecieverFactory _eventSubscriptionRecieverFactory;

        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;


        public AzureMessageQueueFactory(
            CloudBlobClient cloudBlobClient
            , QueueFactoryInterface queueFactory
            , MessageHandlerRespositoryInterface handlerRespository, QueueProcessorTask queueProcessorTask, NamespaceManager namespaceManager, MessagingFactory messagingFactory, EventTopicSenderFactoryInterface eventTopicSenderFactory, EventSubscriptionRecieverFactory eventSubscriptionRecieverFactory)
        {
            _cloudBlobClient = cloudBlobClient;
            _queueFactory = queueFactory;
            _queueProcessorTask = queueProcessorTask;
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
            _eventTopicSenderFactory = eventTopicSenderFactory;
            _eventSubscriptionRecieverFactory = eventSubscriptionRecieverFactory;
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

        public TopicBusInterface GetTopicBus(string topicName)
        {
            //var topicQueue = _eventTopicSenderFactory.GetTopicSender(topicName);
            var serializer = GetCommandSerializerForEndpoint(topicName);
            return new EventTopicBus(serializer);
        }

        /*public EventTopicSenderInterface GetTopicSender(string name)
        {
            if (_namespaceManager.TopicExists(name))
            {
                return new AzureEventTopcSender(_messagingFactory.CreateTopicClient(name));
            }

            _namespaceManager.CreateTopic(name);

            return new AzureEventTopcSender(_messagingFactory.CreateTopicClient(name));
        }*/

        public QueuedMessageProcessor GetProcessorForEndpoint(string queueEndpoint)
        {
            var queue = GetQueueForEndpoint(queueEndpoint);
            var commandSerializer = GetCommandSerializerForEndpoint(queueEndpoint);
            return new QueuedMessageProcessor(queue, commandSerializer, _queueProcessorTask);
        }

        public QueuedMessageProcessor GetProcessorForSubscriptionEndpoint(SubscriptionDetails subscriptionDetails)
        {
            var reciever =_eventSubscriptionRecieverFactory.GetSubscriptionReciever(subscriptionDetails);
            var commandSerializer = GetCommandSerializerForEndpoint(subscriptionDetails.Subscription);
            return new QueuedMessageProcessor(reciever, commandSerializer, _queueProcessorTask);
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