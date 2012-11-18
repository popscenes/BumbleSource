using Website.Infrastructure.Authentication;
using Website.Infrastructure.Query;

namespace Website.Domain.Browser.Query
{
    public static class BrowserQueryServiceExtensions
    {
        public static BrowserInterface FindBrowserByIdentityProvider(this GenericQueryServiceInterface queryService, IdentityProviderCredential credential)
        {
            if (string.IsNullOrWhiteSpace(credential.IdentityProvider)
                || string.IsNullOrWhiteSpace(credential.UserIdentifier))
                return null;

            var prov = queryService.FindById<BrowserIdentityProviderCredential>(credential.GetHash());
            return (prov != null) ?
                queryService.FindById<Website.Domain.Browser.Browser>(prov.BrowserId) :
                null;
        }
        
    }
}