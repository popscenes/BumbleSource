using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SetBrowserPropertyCommandHandler : CommandHandlerInterface<SetBrowserPropertyCommand>
    {
        private readonly BrowserRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SetBrowserPropertyCommandHandler(BrowserRepositoryInterface browserRepository
                                                , UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SetBrowserPropertyCommand command)
        {
            var uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_browserRepository});
            using (uow)
            {
                _browserRepository.UpdateEntity<Website.Domain.Browser.Browser>(
                    command.Browser.Id,
                    browser => 
                    browser.Properties[command.PropertyName] = command.PropertyValue);
            }
            return uow.Successful;
        }
    }
}