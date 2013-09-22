using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace Website.Domain.Browser.Command
{
    internal class SetExternalCredentialCommandHandler : MessageHandlerInterface<SetExternalCredentialCommand>
    {
        private readonly GenericRepositoryInterface _repository;


        public SetExternalCredentialCommandHandler(GenericRepositoryInterface repository)
        {
            _repository = repository;
        }

        public void Handle(SetExternalCredentialCommand command)
        {

                _repository.UpdateEntity<Browser>(command.Credential.BrowserId, 
                    browser =>
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

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _repository };
        }
    }
}
