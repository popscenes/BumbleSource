using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    public class SetExternalCredentialCommand : DefaultCommandBase
    {
        public BrowserIdentityProviderCredential Credential { get; set; }
    }
}
