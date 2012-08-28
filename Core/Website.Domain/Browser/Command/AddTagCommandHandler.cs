using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class AddTagCommandHandler : CommandHandlerInterface<AddTagCommand>
    {
        private readonly BrowserRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public AddTagCommandHandler(BrowserRepositoryInterface browserRepository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(AddTagCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _browserRepository.UpdateEntity<Browser>(command.BrowserId,
                    browser =>
                        {
                            if (!browser.Tags.IsSupersetOf(command.Tags))
                            {
                                browser.Tags.UnionWith(command.Tags);
                                _browserRepository.Store(browser);
                            }
                        });
            }

            return true;
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() {_browserRepository};
        }
    }
}