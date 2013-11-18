using Microsoft.ServiceBus.Messaging;
using Website.Application.Azure.Queue;
using Website.Application.Messaging;
using Website.Application.Queue;

namespace Website.Application.Azure.ServiceBus
{
    public class AzureEventTopcSender : EventTopicSenderInterface
    {
        private readonly TopicClient _topicClient;

        public AzureEventTopcSender(TopicClient topicClient)
        {
            _topicClient = topicClient;
        }

        public void AddMessage(QueueMessageInterface message)
        {
            var azureMsg = message as ServiceBusQueueMessage ?? new ServiceBusQueueMessage(message.Bytes);
            _topicClient.Send(azureMsg.Message);
        }
    }
}