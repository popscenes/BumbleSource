using Website.Domain.Claims;

namespace Website.Domain.Service
{
    public interface DomainEventPublicationServiceInterface
    {
        void Publish<DomainEventType>(DomainEventType subject);
    }
}