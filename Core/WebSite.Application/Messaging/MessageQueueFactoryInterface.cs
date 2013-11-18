using System;
using Website.Infrastructure.Messaging;

namespace Website.Application.Messaging
{
    public interface MessageQueueFactoryInterface 
    {
        MessageBusInterface GetMessageBusForEndpoint(string queueEndpoint);
        void Delete(string queueEndpoint);

        TopicBusInterface GetTopicBus(string name);
        //EventTopicSenderInterface GetTopicSender(string name);

        QueuedMessageProcessor GetProcessorForEndpoint(string queueEndpoint);
        QueuedMessageProcessor GetProcessorForSubscriptionEndpoint(SubscriptionDetails subscriptionDetails);
    }
}