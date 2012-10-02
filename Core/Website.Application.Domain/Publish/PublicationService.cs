using Website.Domain.Service;
using Website.Infrastructure.Publish;

namespace Website.Application.Domain.Publish
{
    public class PublicationService : PublicationServiceInterface
    {
        private readonly PublishBroadcastServiceInterface _broadcastService;

        public PublicationService(PublishBroadcastServiceInterface broadcastService)
        {
            _broadcastService = broadcastService;
        }

        public void Publish<PublishType>(PublishType subject)
        {
            _broadcastService.Broadcast(subject);
        }
    }
}