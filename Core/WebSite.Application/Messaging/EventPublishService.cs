using System.Collections.Generic;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;

namespace Website.Application.Messaging
{
    public class EventPublishService : EventPublishServiceInterface
    {
        private readonly MessageBusInterface _messageBus;

        public EventPublishService([WorkerCommandBus]MessageBusInterface messageBus)
        {
            _messageBus = messageBus;
        }

        public void Publish<PublishType>(PublishType subject) where PublishType : EventInterface
        {
            _messageBus.Send(new EventPublishCommand()
                {
                    Event = subject
                });
        }

        public void PublishAll<EventType>(IEnumerable<EventType> subjects) where EventType : EventInterface
        {
            foreach (var subject in subjects)
            {
                 Publish(subject);   
            }
        }
    }
}