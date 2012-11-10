using Website.Domain.Service;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Publish;

namespace Website.Application.Domain.Publish
{
    public class DomainEventPublishService : DomainEventPublishServiceInterface
    {
        private readonly BroadcastServiceInterface _broadcastService;

        public DomainEventPublishService(BroadcastServiceInterface broadcastService)
        {
            _broadcastService = broadcastService;
        }

        public void Publish<PublishType>(PublishType subject) where PublishType : DomainEventInterface
        {
            _broadcastService.Broadcast(subject);
        }
    }
}