using DotNetOpenAuth.OpenId.RelyingParty;

namespace Website.Infrastructure.Authentication
{
    public interface IdentityProviderInterface
    {
        string Name { get; set; }
        string Identifier { get; set; }

        string RealmUri { get; set; }
        string CallbackUrl { get; set; }

        void RequestAuthorisation();
        IdentityProviderCredential GetCredentials();
    }
}