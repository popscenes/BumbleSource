using System;
using System.Globalization;
using System.Net;
using DotNetOpenAuth.OAuth2;
using Website.Application.Intergrations;
using Website.Infrastructure.Authentication;

namespace Website.Application.Authentication
{
    public class FacebookProvider : IdentityProviderInterface
    {
        private WebServerClient _oauth2Client = null;

        public FacebookProvider(string FacebookAppID, string FacebookAppSecret)
        {
//            _oauth2Client = new WebServerClient(new AuthorizationServerDescription()
//                                                    {
//                                                        
//                                                        TokenEndpoint =
//                                                            new Uri("https://graph.facebook.com/oauth/access_token"),
//                                                        AuthorizationEndpoint =
//                                                            new Uri("https://www.facebook.com/dialog/oauth?scope=user_events,friends_events,publish_stream")
//                                                    }, FacebookAppID, FacebookAppSecret);
        }

        public string CallbackUrl { get; set; }

        public string Name { get; set; }

        public string Identifier { get; set; }

        public string RealmUri { get; set; }

        //public IAuthorizationState AccessToken { get {return _oauth2Client.ProcessUserAuthorization(); } }

        public void RequestAuthorisation()
        {
            throw new NotImplementedException();
//            _oauth2Client
//                .RequestUserAuthorization(returnTo: new Uri(CallbackUrl));
        }

        public IdentityProviderCredential GetCredentials()
        {
            throw new NotImplementedException();
//            var accessToken = _oauth2Client.ProcessUserAuthorization();
//            var user = FacebookUserGet(accessToken.AccessToken);
//            return new IdentityProviderCredential()
//                       {
//                           Email = user.email,
//                           Name = user.name,
//                           UserIdentifier = user.id.ToString(CultureInfo.InvariantCulture),
//                           IdentityProvider = IdentityProviders.FACEBOOK
//
//                       };
        }

        protected FaceBookUser FacebookUserGet(string accessToken)
        {
            FaceBookUser user = null;
            var graph = new FacebookGraph(accessToken);
            user = graph.GetUser();
            return user;
        }
    }
}