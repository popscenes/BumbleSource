using System.Collections.Generic;
using WebSite.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SavedLocationAddCommandHandler : CommandHandlerInterface<SavedLocationAddCommand>
    {
        private readonly BrowserRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedLocationAddCommandHandler(BrowserRepositoryInterface browserRepository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedLocationAddCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _browserRepository.UpdateEntity<Browser>(command.BrowserId, browser => browser.SavedLocations.Add(command.Location));
            }

            return new MsgResponse("Saved Location Add", false).AddCommandId(command);
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _browserRepository };
        }
    }
}
