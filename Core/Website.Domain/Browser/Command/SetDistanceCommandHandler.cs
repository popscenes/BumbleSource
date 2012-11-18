using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SetDistanceCommandHandler: CommandHandlerInterface<SetDistanceCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SetDistanceCommandHandler(GenericRepositoryInterface repository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SetDistanceCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {
                _repository.UpdateEntity<Browser>(command.BrowserId, browser => browser.Distance = command.Distance);
            }

            return true;
        }

    }
}
