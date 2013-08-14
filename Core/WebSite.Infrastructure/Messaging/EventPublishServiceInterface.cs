using Website.Infrastructure.Domain;

namespace Website.Infrastructure.Messaging
{
    public interface EventPublishServiceInterface
    {
        void Publish<DomainEventType>(DomainEventType subject) where DomainEventType : EventInterface;
    }
}