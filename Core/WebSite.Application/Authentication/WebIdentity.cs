using System;
using System.Linq;
using System.Web.Security;
using WebSite.Infrastructure.Authentication;

namespace WebSite.Application.Authentication
{
    public class WebIdentity: WebIdentityInterface
    {
        
        public WebIdentity()
        {
            _identityProvider = "";
            _nameIdentifier = "";
            _name = "";
            _emailAddress = "";
            _isAUthenticated = false;
        }

        public WebIdentity(FormsAuthenticationTicket authTicket)
        {
            var userdataArray = authTicket.UserData.Split(new char[] {'|'}, StringSplitOptions.None);
            if(userdataArray.Count() < 4)
            {
                _isAUthenticated = false;
                return;
            }

            PopulateIdentityFromUserData(userdataArray);
        }

        private void PopulateIdentityFromUserData(String [] userDataArray)
        {
            _identityProvider = userDataArray[0];
            _nameIdentifier = userDataArray[1];
            _name = userDataArray[2];
            _emailAddress = userDataArray[3];

            _isAUthenticated = true;
        }

        private string _emailAddress;

        public string EmailAddress
        {
            get { return _emailAddress; }
        }


        private string _identityProvider;
        
        public string IdentityProvider
        {
            get { return _identityProvider; }
        }

        private string _nameIdentifier;
        
        public string NameIdentifier
        {
            get { return _nameIdentifier; }
        }

        public IdentityProviderCredential ToCredential()
        {
            return new IdentityProviderCredential() { IdentityProvider = IdentityProvider, UserIdentifier = NameIdentifier, Email = _emailAddress, Name = _name};
        }

        public string AuthenticationType
        {
            get { return "forms"; }
        }

        private bool _isAUthenticated;
        public bool IsAuthenticated
        {
            get { return _isAUthenticated; }
        }

        private string _name;
        
        public string Name
        {
            get { return _name; }
        }
    }
}