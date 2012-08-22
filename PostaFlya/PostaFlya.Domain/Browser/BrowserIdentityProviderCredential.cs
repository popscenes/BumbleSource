using System;
using WebSite.Infrastructure.Authentication;

namespace PostaFlya.Domain.Browser
{
    [Serializable]
    public class BrowserIdentityProviderCredential : 
        IdentityProviderCredential
        , BrowserIdInterface
    {
        public string BrowserId { get; set; }
    }
}