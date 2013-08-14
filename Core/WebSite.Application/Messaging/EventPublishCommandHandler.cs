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


        public object Handle(EventPublishCommand command)
        {
            return _broadcastService.Broadcast(command.Event);
        }
    }
}