using System.Collections.Generic;

namespace Website.Infrastructure.Authentication
{
    public interface IdentityProviderServiceInterface
    {
        IEnumerable<IdentityProviderInterface> Get();
        IdentityProviderInterface GetProviderByIdentifier(string providerIdentifier);
    }
}