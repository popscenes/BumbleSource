using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SavedLocationDeleteCommandHandler: CommandHandlerInterface<SavedLocationDeleteCommand>
    {
        private readonly GenericRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedLocationDeleteCommandHandler(GenericRepositoryInterface browserRepository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedLocationDeleteCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(new[] { _browserRepository }))
            {
                _browserRepository.UpdateEntity<Browser>(command.BrowserId, browser => browser.SavedLocations.Remove(command.Location));
            }

            return true;
        }
    }
}
