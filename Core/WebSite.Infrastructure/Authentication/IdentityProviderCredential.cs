using System;
using WebSite.Infrastructure.Util;

namespace WebSite.Infrastructure.Authentication
{
    [Serializable]
    public class IdentityProviderCredential
    {
        public string IdentityProvider { get; set; }
        public string UserIdentifier { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public AccessToken AccessToken { get; set; }

        public override int GetHashCode()
        {
            return IdentityProvider.GetHashCode() ^ UserIdentifier.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IdentityProviderCredential))
                return false;
            var other = obj as IdentityProviderCredential;
            return IdentityProvider.Equals(other.IdentityProvider) && UserIdentifier.Equals(other.UserIdentifier);
        }
        public string GetHash()
        {
            return CryptoUtil.CalculateHash(IdentityProvider + UserIdentifier);
        }

        public override string ToString()
        {
            return IdentityProvider + "|" + UserIdentifier + "|" + Name + "|" + Email;
        }
    }
}