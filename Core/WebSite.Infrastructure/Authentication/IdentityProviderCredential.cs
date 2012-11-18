using System;
using Website.Infrastructure.Util;

namespace Website.Infrastructure.Authentication
{
    public static class IdentityProviderCredentialInterfaceExtensions
    {
        public static void CopyFieldsFrom(this IdentityProviderCredentialInterface target,
            IdentityProviderCredentialInterface source)
        {
            target.IdentityProvider = source.IdentityProvider;
            target.UserIdentifier = source.UserIdentifier;
            target.Name = source.Name;
            target.Email = source.Email;
            if (source.AccessToken != null)
            {
                target.AccessToken = new AccessToken()
                {
                    Expires = source.AccessToken.Expires,
                    Permissions = source.AccessToken.Permissions,
                    Token = source.AccessToken.Token
                };
            }
        }
    }

    public interface IdentityProviderCredentialInterface
    {
        string IdentityProvider { get; set; }
        string UserIdentifier { get; set; }
        string Name { get; set; }
        string Email { get; set; }
        AccessToken AccessToken { get; set; }        
    }

    [Serializable]
    public class IdentityProviderCredential : IdentityProviderCredentialInterface
    {
        public string IdentityProvider { get; set; }
        public string UserIdentifier { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public AccessToken AccessToken { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var identprovider = obj as IdentityProviderCredential;
            if (identprovider == null) return false;
            return Equals(identprovider);
        }

        public string GetHash()
        {
            return CryptoUtil.CalculateHash(IdentityProvider + UserIdentifier);
        }

        public override string ToString()
        {
            return IdentityProvider + "|" + UserIdentifier + "|" + Name + "|" + Email;
        }

        public bool Equals(IdentityProviderCredential other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.IdentityProvider, IdentityProvider) && Equals(other.UserIdentifier, UserIdentifier);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((IdentityProvider != null ? IdentityProvider.GetHashCode() : 0)*397) 
                    ^ (UserIdentifier != null ? UserIdentifier.GetHashCode() : 0);
            }
        }
    }
}