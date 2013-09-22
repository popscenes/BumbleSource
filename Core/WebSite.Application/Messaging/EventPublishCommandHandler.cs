using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;

namespace Website.Application.Messaging
{
    public class EventPublishCommandHandler : MessageHandlerInterface<EventPublishCommand>
    {
        private readonly BroadcastServiceInterface _broadcastService;

        public EventPublishCommandHandler(BroadcastServiceInterface broadcastService)
        {
            _broadcastService = broadcastService;
        }


        public void Handle(EventPublishCommand command)
        {
            _broadcastService.Broadcast(command.Event);
        }
    }
}