using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    internal class SavedLocationSelectCommandHandler: CommandHandlerInterface<SavedLocationSelectCommand>
    {
        private readonly BrowserRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedLocationSelectCommandHandler(BrowserRepositoryInterface browserRepository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedLocationSelectCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _browserRepository.UpdateEntity<Browser>(command.BrowserId, browser => browser.DefaultLocation = command.Location);
            }

            return new MsgResponse("Saved Location Select", false).AddCommandId(command);
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _browserRepository };
        }
    }
}
