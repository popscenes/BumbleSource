using Website.Infrastructure.Messaging;

namespace Website.Application.Messaging
{
    public interface MessageQueueFactoryInterface 
    {
        MessageBusInterface GetMessageBusForEndpoint(string queueEndpoint);
        void Delete(string queueEndpoint);
        QueuedMessageProcessor GetProcessorForEndpoint(string queueEndpoint);
    }
}