using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Website.Application.Intergrations;
using Website.Infrastructure.Authentication;

namespace Website.Application.Authentication
{
    public class HomeRolledFacebookProvider : IdentityProviderInterface
    {
        private readonly HttpContextBase _httpContext;
        private readonly string _facebookAppId;
        private readonly string _facebookAppSecret;
        public string Name { get; set; }

        public string Identifier { get; set; }

        public string RealmUri { get; set; }

        public string CallbackUrl { get; set; }

        //testing purposes

        //public AccessToken AccessToken { get; set; }

        //public FaceBookUser FacebookUser
        //{
        //    get { return _facebookUser ?? (_facebookUser = FacebookUserGet()); }
        //    set { _facebookUser = value; }
        //}

        public HomeRolledFacebookProvider(HttpContextBase httpContext, String facebookAppId, String facebookAppSecret)
        {
            _httpContext = httpContext;
            _facebookAppId = facebookAppId;
            _facebookAppSecret = facebookAppSecret;
        }

        private String ReadWebResponseStream(WebResponse response)
        {
            using (response)
            {
                using (var responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream);
                    var responsString = reader.ReadToEnd();
                    return responsString;
                }
            }
        }

        private string GetTokenfacebookPermissions(string token)
        {
            var graphApi = new FacebookGraph(token);
            var perms = graphApi.GetUserPermission();
            return String.Join(",", perms[0].Select(_ => _.Key).ToArray());
        }

        private String RequestAccessToken(String code)
        {
            var accessCodeUrl = String.Format("https://graph.facebook.com/oauth/access_token?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}"
                , Uri.EscapeDataString(_facebookAppId), Uri.EscapeDataString(_facebookAppSecret), Uri.EscapeDataString(code), Uri.EscapeDataString(CallbackUrl));
            var tokenRequest = WebRequest.Create(accessCodeUrl);
            var response = tokenRequest.GetResponse();
            return ReadWebResponseStream(response);
        }

        private AccessToken ProcessAccessTokenResponse(String responseData)
        {
            var responseVals = HttpUtility.ParseQueryString(responseData);
            var token = responseVals["access_token"];
            var expires = responseVals["expires"];
            var perms = GetTokenfacebookPermissions(token);
            var seconds = Double.Parse(expires);

            return new AccessToken() { Token = token, Expires = DateTime.Now.AddSeconds(seconds), Permissions = perms };
        }

        private AccessToken GetAccessTokenFromFacebookResponse()
        {
            if(_httpContext.Request["error"] != null)
            {
                var errorMsg = _httpContext.Request["error_description"];
                return null;
                //throw new ArgumentException(errorMsg);
            }

            var code = _httpContext.Request["code"];
            var responseData = RequestAccessToken(code);
            return ProcessAccessTokenResponse(responseData);
        }

        private FaceBookUser FacebookUserGet(AccessToken token)
        {
            FaceBookUser user = null;
            var graph = new FacebookGraph(token.Token);
            user = graph.GetUser();
            return user;
        }

        public void RequestAuthorisation()
        {
            string url = String.Format("https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&scope={2}"
                , Uri.EscapeDataString(_facebookAppId), Uri.EscapeDataString(CallbackUrl), Uri.EscapeDataString("email,user_events,friends_events,publish_stream"));
            _httpContext.Response.Redirect(url);
        }

        public IdentityProviderCredential GetCredentials()
        {
            var token = GetAccessTokenFromFacebookResponse();
            if (token == null)
                return null;
            var user = FacebookUserGet(token);

            return new IdentityProviderCredential()
            {
                Email = user.email,
                Name = user.name,
                UserIdentifier = user.id.ToString(CultureInfo.InvariantCulture),
                IdentityProvider = IdentityProviders.FACEBOOK,
                AccessToken = token

            };
        }
    }
}
