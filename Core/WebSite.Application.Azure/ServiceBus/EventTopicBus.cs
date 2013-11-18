using Ninject;
using Ninject.Syntax;
using Website.Application.Azure.Queue;
using Website.Application.Messaging;
using Website.Infrastructure.Domain;

namespace Website.Application.Azure.ServiceBus
{
    public class EventTopicBus : TopicBusInterface
    {
        //private readonly EventTopicSenderInterface _eventTopicSender;
        private readonly MessageSerializerInterface _messageSerializer;
        [Inject]
        private readonly IResolutionRoot _resolver;

        public EventTopicBus(MessageSerializerInterface messageSerializer)
        {
            _messageSerializer = messageSerializer;
        }

        public object Send<EventType>(EventType @event) where EventType : class, EventInterface
        {
            var message = _messageSerializer.ToByteArray(@event);
            var sendmessage = new ServiceBusQueueMessage(message);

            var topicSender = _resolver.Get<EventTopicSenderInterface>(metadata => metadata.Get<string>("Topic") == @event.GetType().Name);
            topicSender.AddMessage(sendmessage);
            return true;
        }
    }
}