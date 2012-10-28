using Website.Infrastructure.Authentication;
using Website.Infrastructure.Query;

namespace Website.Domain.Browser.Query
{
    public interface BrowserQueryServiceInterface : GenericQueryServiceInterface
    {
        BrowserInterface FindByIdentityProvider(IdentityProviderCredential credential);
    }
}