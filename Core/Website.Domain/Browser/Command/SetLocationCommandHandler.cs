using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SetLocationCommandHandler : CommandHandlerInterface<SetLocationCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SetLocationCommandHandler(GenericRepositoryInterface repository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SetLocationCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {
                _repository.UpdateEntity<Browser>(command.BrowserId
                                                , browser => browser.DefaultLocation = command.Location);
            }

            return true;
        }
    }
}
