using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SetExternalCredentialCommandHandler : CommandHandlerInterface<SetExternalCredentialCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SetExternalCredentialCommandHandler(GenericRepositoryInterface repository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SetExternalCredentialCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _repository.UpdateEntity<Browser>(command.Credential.BrowserId, browser =>
                                                                                           {
                                                                                               browser.
                                                                                                   ExternalCredentials.
                                                                                                   Remove(
                                                                                                       command.
                                                                                                           Credential);
                                                                                               browser.
                                                                                                   ExternalCredentials.
                                                                                                   Add(
                                                                                                       command.
                                                                                                           Credential);
                                                                                           });
            }

            return true;
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _repository };
        }
    }
}
