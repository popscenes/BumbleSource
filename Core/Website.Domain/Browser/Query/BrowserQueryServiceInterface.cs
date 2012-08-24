using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Query;

namespace Website.Domain.Browser.Query
{
    public interface BrowserQueryServiceInterface : GenericQueryServiceInterface
    {
        BrowserInterface FindByIdentityProvider(IdentityProviderCredential credential);
        BrowserInterface FindByHandle(string handle);
    }
}