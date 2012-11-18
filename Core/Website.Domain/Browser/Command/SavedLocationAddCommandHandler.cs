using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SavedLocationAddCommandHandler : CommandHandlerInterface<SavedLocationAddCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedLocationAddCommandHandler(GenericRepositoryInterface repository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedLocationAddCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _repository.UpdateEntity<Browser>(command.BrowserId, browser => browser.SavedLocations.Add(command.Location));
            }

            return new MsgResponse("Saved Location Add", false).AddCommandId(command);
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _repository };
        }
    }
}
