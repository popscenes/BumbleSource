using System;
using Website.Infrastructure.Authentication;

namespace Website.Domain.Browser
{
    [Serializable]
    public class BrowserIdentityProviderCredential : 
        IdentityProviderCredential
        , BrowserIdInterface
    {
        public string BrowserId { get; set; }
    }
}