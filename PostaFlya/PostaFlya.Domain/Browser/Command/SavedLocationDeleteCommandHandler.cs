using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    internal class SavedLocationDeleteCommandHandler: CommandHandlerInterface<SavedLocationDeleteCommand>
    {
        private readonly BrowserRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedLocationDeleteCommandHandler(BrowserRepositoryInterface browserRepository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedLocationDeleteCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _browserRepository.UpdateEntity<Browser>(command.BrowserId, browser => browser.SavedLocations.Remove(command.Location));
            }

            return true;
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _browserRepository };
        }
    }
}
