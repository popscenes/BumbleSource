using System;
using System.Linq;
using System.Web.Security;
using Website.Infrastructure.Authentication;

namespace Website.Application.Authentication
{
    public class WebIdentity: WebIdentityInterface
    {
        
        public WebIdentity()
        {
            _identityProvider = "";
            _nameIdentifier = "";
            _name = "";
            _emailAddress = "";
            _isAuthenticated = false;
        }

        public WebIdentity(String userData)
        {
            Init(userData);
        }

        public WebIdentity(FormsAuthenticationTicket authTicket)
        {
            Init(authTicket.UserData);
        }

        private void Init(string userData)
        {
            var userdataArray = userData.Split(new[] { IdentityProviderCredential.Delimiter }, StringSplitOptions.None);
            if (userdataArray.Count() < 2)
            {
                _isAuthenticated = false;
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

            _isAuthenticated = true;
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

        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
        }

        private string _name;
        
        public string Name
        {
            get { return _name; }
        }
    }
}