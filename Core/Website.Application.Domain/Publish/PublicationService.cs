using Website.Application.Binding;
using Website.Application.Publish;
using Website.Domain.Service;
using Website.Infrastructure.Command;

namespace Website.Application.Domain.Publish
{
    public class PublicationService : PublicationServiceInterface
    {
        private readonly CommandBusInterface _commandBus;

        public PublicationService([WorkerCommandBus]CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public void Publish<PublishType>(PublishType subject)
        {
            _commandBus.Send(new PublishCommand() { PublishObject = subject });
        }
    }
}