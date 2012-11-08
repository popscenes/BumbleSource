using Website.Domain.Service;
using Website.Infrastructure.Publish;

namespace Website.Application.Domain.Publish
{
    public class DomainEventPublicationService : DomainEventPublicationServiceInterface
    {
        private readonly BroadcastServiceInterface _broadcastService;

        public DomainEventPublicationService(BroadcastServiceInterface broadcastService)
        {
            _broadcastService = broadcastService;
        }

        public void Publish<PublishType>(PublishType subject)
        {
            _broadcastService.Broadcast(subject);
        }
    }
}