using Website.Infrastructure.Authentication;
using Website.Infrastructure.Query;

namespace Website.Domain.Browser.Query
{
    public class FindBrowserByIdentityProviderQuery : QueryInterface
    {
        public IdentityProviderCredential Credential { get; set; }
    }
}