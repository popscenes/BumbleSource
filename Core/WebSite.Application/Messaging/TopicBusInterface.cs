using Website.Infrastructure.Domain;

namespace Website.Application.Messaging
{
    public interface TopicBusInterface
    {
        object Send<EventType>(EventType @event) where EventType : class, EventInterface;
    }
}