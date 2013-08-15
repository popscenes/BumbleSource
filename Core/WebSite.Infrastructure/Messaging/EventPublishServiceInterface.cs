using System.Collections.Generic;
using Website.Infrastructure.Domain;

namespace Website.Infrastructure.Messaging
{
    public interface EventPublishServiceInterface
    {
        void Publish<EventType>(EventType subject) where EventType : EventInterface;
        void PublishAll<EventType>(IEnumerable<EventType> subjects) where EventType : EventInterface;
    }
}