using Website.Domain.Claims;
using Website.Infrastructure.Domain;

namespace Website.Domain.Service
{
    public interface DomainEventPublishServiceInterface
    {
        void Publish<DomainEventType>(DomainEventType subject) where DomainEventType : DomainEventInterface;
    }
}