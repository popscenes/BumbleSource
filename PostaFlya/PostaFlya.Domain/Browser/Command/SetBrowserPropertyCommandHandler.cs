using Website.Domain.Browser.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace PostaFlya.Domain.Browser.Command
{
    internal class SetBrowserPropertyCommandHandler : MessageHandlerInterface<SetBrowserPropertyCommand>
    {
        private readonly GenericRepositoryInterface _repository;

        public SetBrowserPropertyCommandHandler(GenericRepositoryInterface repository)
        {
            _repository = repository;
        }

        public void Handle(SetBrowserPropertyCommand command)
        {
            _repository.UpdateEntity<PostaFlya.Domain.Browser.Browser>(
                command.Browser.Id,
                browser => 
                browser.Properties[command.PropertyName] = command.PropertyValue);
            
        }
    }
}