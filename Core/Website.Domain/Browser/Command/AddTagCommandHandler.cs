using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class AddTagCommandHandler : CommandHandlerInterface<AddTagCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public AddTagCommandHandler(GenericRepositoryInterface repository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(AddTagCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {
                _repository.UpdateEntity<Browser>(command.BrowserId,
                    browser =>
                        {
                            if (!browser.Tags.IsSupersetOf(command.Tags))
                            {
                                browser.Tags.UnionWith(command.Tags);
                                _repository.Store(browser);
                            }
                        });
            }

            return true;
        }
    }
}