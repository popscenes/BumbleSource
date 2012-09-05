using System.Threading.Tasks;
using Website.Infrastructure.Command;
using Website.Infrastructure.Publish;

namespace Website.Application.Publish
{
    internal class PublishCommandHandler : CommandHandlerInterface<PublishCommand>
    {
        private readonly PublishBroadcastServiceInterface _broadcastService;

        public PublishCommandHandler(PublishBroadcastServiceInterface broadcastService)
        {
            _broadcastService = broadcastService;
        }

        readonly ParallelOptions _parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };
        public object Handle(PublishCommand command)
        {
            return _broadcastService.Broadcast(command.PublishObject);
        }
    }
}