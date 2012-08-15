using System;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Domain.Browser.Query
{
    public interface BrowserQueryServiceInterface : GenericQueryServiceInterface<BrowserInterface>
    {
        BrowserInterface FindByIdentityProvider(IdentityProviderCredential credential);
        BrowserInterface FindByHandle(string handle);
    }
}