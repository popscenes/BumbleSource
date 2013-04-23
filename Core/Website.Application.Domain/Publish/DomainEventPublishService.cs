using Website.Application.Binding;
using Website.Application.Domain.Publish.Command;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Application.Domain.Publish
{
    public class DomainEventPublishService : DomainEventPublishServiceInterface
    {
        private readonly CommandBusInterface _commandBus;

        public DomainEventPublishService([WorkerCommandBus]CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public void Publish<PublishType>(PublishType subject) where PublishType : DomainEventInterface
        {
            _commandBus.Send(new DomainEventPublishCommand()
                {
                    Event = subject
                });
        }
    }
}