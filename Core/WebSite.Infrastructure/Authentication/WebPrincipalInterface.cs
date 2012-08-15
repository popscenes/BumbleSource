using System;
using System.Security.Principal;

namespace WebSite.Infrastructure.Authentication
{
    public interface WebPrincipalInterface : IPrincipal
    {
        String Name { get; }
        String EmailAddress { get; }
        String IdentityProvider { get; }
        String NameIdentifier { get; }
//        String UniqueIdentifier { get; }
        IdentityProviderCredential ToCredential();
    }
}