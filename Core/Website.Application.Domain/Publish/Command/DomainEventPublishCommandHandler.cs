using Website.Infrastructure.Command;
using Website.Infrastructure.Publish;

namespace Website.Application.Domain.Publish.Command
{
    internal class DomainEventPublishCommandHandler : CommandHandlerInterface<DomainEventPublishCommand>
    {
        private readonly BroadcastServiceInterface _broadcastService;

        public DomainEventPublishCommandHandler(BroadcastServiceInterface broadcastService)
        {
            _broadcastService = broadcastService;
        }


        public object Handle(DomainEventPublishCommand command)
        {
            return _broadcastService.Broadcast(command.Event);
        }
    }
}