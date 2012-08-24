using Website.Domain.Command;

namespace Website.Domain.Browser.Command
{
    public class SetExternalCredentialCommand : DomainCommandBase
    {
        public BrowserIdentityProviderCredential Credential { get; set; }
    }
}
