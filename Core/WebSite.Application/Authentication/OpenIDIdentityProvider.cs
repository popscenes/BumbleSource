using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.RelyingParty;
using Website.Infrastructure.Authentication;

namespace Website.Application.Authentication
{
    public class OpenIdIdentityProvider : IdentityProviderInterface
    {
        public string Name{get; set; }
        public string Identifier { get; set;}

        public string RealmUri { get; set; }
        public string CallbackUrl { get; set; }

        private IAuthenticationResponse _authResponse;
        public IAuthenticationResponse AuthResponse 
        { 
            get { 
                    throw new NotImplementedException();
//                return _authResponse ?? 
//                (_authResponse = _openIdRelyingParty.GetResponse()); 
            
            }

            set { _authResponse = value; } 
        }

        public string LoginUrl
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

        private OpenIdRelyingParty _openIdRelyingParty;
        

        public OpenIdIdentityProvider()
        {
            _openIdRelyingParty = new OpenIdRelyingParty();
        }

        public void RequestAuthorisation()
        {
            throw new NotImplementedException();
//            var req = _openIdRelyingParty.CreateRequest(Identifier, new Realm(RealmUri), new Uri(CallbackUrl));
//            var fetch = new FetchRequest();
//            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Email, true));
//            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.First, true));
//            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.Last, true));
//            req.AddExtension(fetch);
//            req.RedirectingResponse.Send();
        }

        public IdentityProviderCredential GetCredentials()
        {
            

            var fetch = AuthResponse.GetExtension<FetchResponse>();

            IList<string> emailAddresses = new List<string>();
            IList<string> firstNames = new List<string>();
            IList<string> lastName = new List<string>();

            if (fetch != null)
            {
                if (fetch.Attributes.Count(_ => _.TypeUri == WellKnownAttributes.Contact.Email) > 0)                
                    emailAddresses = fetch.Attributes[WellKnownAttributes.Contact.Email].Values;
                if (fetch.Attributes.Count(_ => _.TypeUri == WellKnownAttributes.Name.First) > 0)                
                    firstNames = fetch.Attributes[WellKnownAttributes.Name.First].Values;
                if (fetch.Attributes.Count(_ => _.TypeUri == WellKnownAttributes.Name.Last) > 0)                
                    lastName = fetch.Attributes[WellKnownAttributes.Name.Last].Values;

            }
            string identifer = AuthResponse.ClaimedIdentifier;
            return new IdentityProviderCredential() 
            { 
                Email = emailAddresses.FirstOrDefault(),
                Name = firstNames.FirstOrDefault() == "" &&  lastName.FirstOrDefault() == "" ? "" : firstNames.FirstOrDefault() + " " + lastName.FirstOrDefault(),
                UserIdentifier = String.IsNullOrWhiteSpace(emailAddresses.FirstOrDefault()) ? identifer : emailAddresses.FirstOrDefault(),
                IdentityProvider = AuthResponse.Provider.Uri.Host
            
            };

        }


       
    }
}
