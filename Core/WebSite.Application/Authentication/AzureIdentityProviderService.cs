using System;
using System.Collections.Generic;
using Website.Infrastructure.Authentication;

namespace Website.Application.Authentication
{
    public class AzureIdentityProviderService : IdentityProviderServiceInterface
    {
        public IEnumerable<IdentityProviderInterface> Get()
        {
//            WSFederationAuthenticationModule fam = FederatedAuthentication.WSFederationAuthenticationModule;
//            HrdRequest request = new HrdRequest(fam.Issuer, fam.Realm);
//
//            WebClient client = new WebClient();
//            client.Encoding = Encoding.UTF8;
//
//            string response = client.DownloadString(request.GetUrlWithQueryString());
//
//            JavaScriptSerializer serializer = new JavaScriptSerializer();
//            return serializer.Deserialize<List<AzureAcsIdentityProvider>>(response);
            throw new NotImplementedException();
        }


        public IdentityProviderInterface GetProviderByIdentifier(string providerIdentifier)
        {
            throw new NotImplementedException();
        }
    }
}
