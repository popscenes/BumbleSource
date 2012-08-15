using System.Collections.Generic;

namespace WebSite.Infrastructure.Authentication
{
    public interface IdentityProviderServiceInterface
    {
        IEnumerable<IdentityProviderInterface> Get();
        IdentityProviderInterface GetProviderByIdentifier(string providerIdentifier);
    }
}