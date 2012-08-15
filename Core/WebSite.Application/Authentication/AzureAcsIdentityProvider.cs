using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OpenId.RelyingParty;
using WebSite.Infrastructure.Authentication;

namespace WebSite.Application.Authentication
{
    class AzureAcsIdentityProvider : IdentityProviderInterface
    {
        public AzureAcsIdentityProvider()
        {

        }

        public string Name { get; set; }

        public string LoginUrl{ get; set; }

        public string LogoutUrl { get; set; }

        public string ImageUrl { get; set; }


        public string Identifier
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }




        public void RequestAuthorisation()
        {
            throw new NotImplementedException();
        }

        public IdentityProviderCredential GetCredentialsFromAuthResponse()
        {
            throw new NotImplementedException();
        }


        public IdentityProviderCredential GetCredentials()
        {
            throw new NotImplementedException();
        }

        public string RealmUri
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string CallbackUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
