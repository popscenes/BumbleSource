using Microsoft.WindowsAzure.Storage.Blob;
using Website.Application.Azure.Content;
using Website.Application.Messaging;

namespace Website.Application.Azure.ServiceBus
{
    public class AzureMessageEventTopicBusFactory
    {
        private readonly EventTopicSenderFactoryInterface _eventTopicSenderFactory;
        private readonly CloudBlobClient _cloudBlobClient;

        public AzureMessageEventTopicBusFactory(EventTopicSenderFactoryInterface eventTopicSenderFactory, CloudBlobClient cloudBlobClient)
        {
            _eventTopicSenderFactory = eventTopicSenderFactory;
            _cloudBlobClient = cloudBlobClient;
        }

        public EventTopicBus GetTopicBus(string topinName)
        {
            //var topicQueue = _eventTopicSenderFactory.GetTopicSender(topinName);
            var serializer = GetSerializerForEndpoint(topinName);
            return new EventTopicBus(serializer);
        }

        private MessageSerializerInterface GetSerializerForEndpoint(string topinName)
        {
            var queueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(topinName));
            queueStorage.CreateIfNotExists();
            return new DataBusMessageSerializer(queueStorage);
        }
    }
}