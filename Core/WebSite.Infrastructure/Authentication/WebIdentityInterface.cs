using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace WebSite.Infrastructure.Authentication
{
    public interface WebIdentityInterface: IIdentity
    {
        String EmailAddress { get; }
        String IdentityProvider { get; }
        String NameIdentifier { get; }
        //        String UniqueIdentifier { get; }
        IdentityProviderCredential ToCredential();
    }
}
