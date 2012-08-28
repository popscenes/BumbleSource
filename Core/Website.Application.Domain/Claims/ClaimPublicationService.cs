using Website.Application.Binding;
using Website.Application.Publish;
using Website.Infrastructure.Command;
using Website.Domain.Claims;
using Website.Domain.Service;

namespace Website.Application.Domain.Claims
{
    public class ClaimPublicationService : ClaimPublicationServiceInterface
    {
        private readonly CommandBusInterface _commandBus;

        public ClaimPublicationService([WorkerCommandBus]CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public void Publish(Claim claim)
        {
            _commandBus.Send(new PublishCommand() {PublishObject = claim});
        }
    }
}