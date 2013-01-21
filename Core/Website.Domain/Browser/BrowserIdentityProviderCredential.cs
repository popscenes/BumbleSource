using System;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Domain;

namespace Website.Domain.Browser
{
    public static class BrowserIdentityProviderCredentialInterfaceExtensions
    {
        public static void CopyFieldsFrom(this BrowserIdentityProviderCredentialInterface target,
                                          BrowserIdentityProviderCredentialInterface source)
        {
            IdentityProviderCredentialInterfaceExtensions.CopyFieldsFrom(target, source);
            BrowserIdInterfaceExtensions.CopyFieldsFrom(target, source);
        }
    }
    public interface BrowserIdentityProviderCredentialInterface : IdentityProviderCredentialInterface
        , EntityIdInterface, BrowserIdInterface
    {
    }

    [Serializable]
    public class BrowserIdentityProviderCredential :
        IdentityProviderCredential, BrowserIdentityProviderCredentialInterface
    {
        public string BrowserId { get; set; }
        public string Id { get { return ToUniqueString(); } set { } }
        public string FriendlyId { get { return ToString(); } set { } }
    }
}