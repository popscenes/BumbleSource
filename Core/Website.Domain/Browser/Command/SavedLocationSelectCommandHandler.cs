using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SavedLocationSelectCommandHandler: CommandHandlerInterface<SavedLocationSelectCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedLocationSelectCommandHandler(GenericRepositoryInterface repository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedLocationSelectCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {
                _repository.UpdateEntity<Browser>(command.BrowserId, browser => browser.DefaultLocation = command.Location);
            }

            return new MsgResponse("Saved Location Select", false).AddCommandId(command);
        }

    }
}
