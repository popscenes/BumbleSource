using System;
using Website.Infrastructure.Util;

namespace Website.Infrastructure.Authentication
{
    [Serializable]
    public class IdentityProviderCredential
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

        public void CopyFieldsFrom(IdentityProviderCredential credential)
        {
            IdentityProvider = credential.IdentityProvider;
            UserIdentifier = credential.UserIdentifier;
            Name = credential.Name;
            Email = credential.Email;
            if (credential.AccessToken != null)
            {
                AccessToken = new AccessToken()
                                  {
                                      Expires = credential.AccessToken.Expires,
                                      Permissions = credential.AccessToken.Permissions,
                                      Token = credential.AccessToken.Token
                                  };
            }
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