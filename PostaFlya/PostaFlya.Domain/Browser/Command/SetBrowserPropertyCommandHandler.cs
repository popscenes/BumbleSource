using Website.Domain.Browser.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace PostaFlya.Domain.Browser.Command
{
    internal class SetBrowserPropertyCommandHandler : MessageHandlerInterface<SetBrowserPropertyCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SetBrowserPropertyCommandHandler(GenericRepositoryInterface repository
                                                , UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SetBrowserPropertyCommand command)
        {
            var uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository});
            using (uow)
            {
                _repository.UpdateEntity<PostaFlya.Domain.Browser.Browser>(
                    command.Browser.Id,
                    browser => 
                    browser.Properties[command.PropertyName] = command.PropertyValue);
            }
            return uow.Successful;
        }
    }
}