using System;
using System.Security.Principal;
using WebSite.Infrastructure.Authentication;

namespace WebSite.Application.Authentication
{
    public class AzureWebPrincipal : WebPrincipalInterface
    {
        private readonly IPrincipal _innerPrincipal;

        public AzureWebPrincipal(IPrincipal currentPrincipal)
        {
          _innerPrincipal = currentPrincipal;
        }

        protected string _name;

        public string Name
        {
            get
            {
                if(String.IsNullOrEmpty(_name))
                {
                    _name = LookupClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                }
                return _name;
            }
        }

        protected string _emailAddress;

        public string EmailAddress
        {
            get
            {
                if (String.IsNullOrEmpty(_emailAddress))
                {
                    _emailAddress = LookupClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                }
                return _emailAddress;
            }
        }

        protected string _identityProvider;
        
        public string IdentityProvider
        {
            get
            {
                if (String.IsNullOrEmpty(_identityProvider))
                {
                    _identityProvider = LookupClaim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider");
                }
                return _identityProvider;
            }
        }

        protected string _nameIdentifier;
        
        public string NameIdentifier
        {
            get
            {
                if (String.IsNullOrEmpty(_nameIdentifier))
                {
                    _nameIdentifier = LookupClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                }
                return _nameIdentifier;
            }
        }

        public IdentityProviderCredential ToCredential()
        {
            return new IdentityProviderCredential() { IdentityProvider = IdentityProvider, UserIdentifier = NameIdentifier};
        }

//        protected string _uniqueIdentifier;
//        
//        public string UniqueIdentifier
//        {
//            get
//            {
//                if (String.IsNullOrEmpty(_uniqueIdentifier)
//                    && !string.IsNullOrWhiteSpace(IdentityProvider)
//                    && !string.IsNullOrWhiteSpace(NameIdentifier))
//                {
//                    _uniqueIdentifier = CryptoUtil.CalculateHash(IdentityProvider + NameIdentifier);
//                }
//                return _uniqueIdentifier;
//            }
//        }

        public System.Security.Principal.IIdentity Identity
        {
            get { return _innerPrincipal.Identity;  }
        }

        public bool IsInRole(string role)
        {
            return true;
        }

        private string LookupClaim(string claim)
        {
//            var claimsId = Identity as IClaimsIdentity;
//            if (claimsId == null) return string.Empty;
//            var claims = claimsId.Claims.FindAll(
//              claimValue => claimValue.ClaimType == claim);
//            if (claims.Count < 1) return string.Empty;
//            return claims.First().Value;
            throw new NotImplementedException();
            
        }
    }
}
