using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId.RelyingParty;
using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using WebSite.Application.Authentication;
using WebSite.Test.Common;
using IAuthenticationRequest = DotNetOpenAuth.OpenId.Provider.IAuthenticationRequest;
using WebSite.Infrastructure.Authentication;

namespace PostaFlya.WebSite.Tests.Authentication
{

    [TestFixture]
    class OpenIdIdentityProviderTests
    {

        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        private IAuthenticationResponse _authResponseWithFetch = null;
        private IAuthenticationResponse _authResponseNoFetch = null;


        protected IAuthenticationResponse AuthenticationResponseGet(bool withfetchData)
        {
            Identifier identifier = null;
            Identifier.TryParse("https://www.google.com/accounts/o8/id?id=AItOawm_Gh8gr9GGjVB0sejGiPeRIULA0dlPFNM", out identifier);

            var fetchResponse = new FetchResponse();
            fetchResponse.Attributes.Add(new AttributeValues("http://axschema.org/contact/email",
                                                                 new string[] { "teddymccuddles@gmail.com" }));
            if (withfetchData)
            {
                fetchResponse.Attributes.Add(new AttributeValues("http://axschema.org/namePerson/first",
                                                                 new string[] {"Anthony"}));
                fetchResponse.Attributes.Add(new AttributeValues("http://axschema.org/namePerson/last",
                                                                 new string[] {"Borg"}));
            }


            var authResponseMock = Kernel.GetMock<IAuthenticationResponse>();
            authResponseMock.Setup(_ => _.Status).Returns(AuthenticationStatus.Authenticated);
            authResponseMock.Setup(_ => _.ClaimedIdentifier).Returns(identifier);
            
            
            authResponseMock.Setup(_ => _.GetExtension<FetchResponse>()).Returns(fetchResponse);
            authResponseMock.Setup(_ => _.Provider.Uri).Returns(new Uri("https://www.google.com/accounts/o8/"));

            return authResponseMock.Object;
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            HttpContextCurrentCreate("http://localhost/", "");
        }

        protected void HttpContextCurrentCreate(string requestUrl, string requestQueryString)
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://localhost:1805/account/Authenticate", requestQueryString),
                new HttpResponse(new StringWriter())
                );

            // User is logged in 
            HttpContext.Current.User = new GenericPrincipal(
                new GenericIdentity("username"),
                new string[0]
                );

            HttpContext.Current.Request.RequestType = "GET";
        }

        protected string OpenIDProviderResponseGet()
        {
            HttpRequestWrapper req = new HttpRequestWrapper(new HttpRequest("", "https://www.google.com/accounts/o8/id", "openid.claimed_id=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.identity=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.assoc_handle=AMlYA9UoocC5z_RMP-QTpNniEE5qVOc5lDXHEzivPwjQq0tZfoDT6my_Sv4LQ4D_GNGTfUJX&openid.return_to=http%3A%2F%2Flocalhost%3A1805%2Faccount%2FAuthenticate%3Fdnoa.userSuppliedIdentifier%3Dhttps%253A%252F%252Fwww.google.com%252Faccounts%252Fo8%252Fid&openid.realm=http%3A%2F%2Flocalhost%3A1805%2F&openid.mode=checkid_setup&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.ns.alias3=http%3A%2F%2Fopenid.net%2Fsrv%2Fax%2F1.0&openid.alias3.required=alias1%2Calias2%2Calias3&openid.alias3.mode=fetch_request&openid.alias3.type.alias1=http%3A%2F%2Faxschema.org%2Fcontact%2Femail&openid.alias3.count.alias1=1&openid.alias3.type.alias2=http%3A%2F%2Faxschema.org%2FnamePerson%2Ffirst&openid.alias3.count.alias2=1&openid.alias3.type.alias3=http%3A%2F%2Faxschema.org%2FnamePerson%2Flast&openid.alias3.count.alias3=1"));
            var identityprovider = new OpenIdProvider();

            var request = identityprovider.GetRequest(req);
            var authReq = request as IAuthenticationRequest;
            authReq.ClaimedIdentifier = "kjhkjh";
            authReq.IsAuthenticated = true;
            identityprovider.SendResponse(request);

            return HttpContext.Current.Response.RedirectLocation;
        }

        [Test]
        public void OpenIdIdentityProviderTestRequestAuthorisationGoogle()
        {
            var httpCtx = Kernel.GetMock<HttpContextBase>();

            //HttpContext.Current = new HttpContext(httpCtx.Object.Request,httpCtx.Object.Response);
            var openIDprovider = new OpenIdIdentityProvider()
                                     {
                                         Name = IdentityProviders.GOOGLE,
                                         Identifier = "https://www.google.com/accounts/o8/id",
                                         CallbackUrl = "http://localhost/Account/Authresponse",
                                         RealmUri = "http://localhost/"

                                     };

            openIDprovider.RequestAuthorisation();

           

            Assert.IsTrue(HttpContext.Current.Response.RedirectLocation.Contains("https://www.google.com/accounts/o8/ud?"));
        }

        [Test]
        public void OpenIdIdentityProviderTestGetCredentialsGoogleWithAllData()
        {
            _authResponseWithFetch = AuthenticationResponseGet(true);
            
            var openIDprovider = new OpenIdIdentityProvider()
            {
                Name = IdentityProviders.GOOGLE,
                Identifier = "https://www.google.com/accounts/o8/id",
                CallbackUrl = "http://localhost:1805/account/Authenticate",
                RealmUri = "http://localhost:1805/"

            };

            openIDprovider.AuthResponse = _authResponseWithFetch;

            var credentials = openIDprovider.GetCredentials();
            Assert.AreEqual(credentials.Email, "teddymccuddles@gmail.com");
            Assert.AreEqual(credentials.Name, "Anthony Borg");
            Assert.AreEqual(credentials.UserIdentifier, "teddymccuddles@gmail.com");
            Assert.AreEqual(credentials.IdentityProvider, "www.google.com"); 
        }

        [Test]
        public void OpenIdIdentityProviderTestGetCredentialsGoogle()
        {
            _authResponseNoFetch = AuthenticationResponseGet(false);

            var openIDprovider = new OpenIdIdentityProvider()
            {
                Name = IdentityProviders.GOOGLE,
                Identifier = "https://www.google.com/accounts/o8/id",
                CallbackUrl = "http://localhost:1805/account/Authenticate",
                RealmUri = "http://localhost:1805/"

            };

            openIDprovider.AuthResponse = _authResponseNoFetch;

            var credentials = openIDprovider.GetCredentials();
            Assert.AreEqual(credentials.Email, "teddymccuddles@gmail.com");
            Assert.IsTrue(String.IsNullOrWhiteSpace(credentials.Name));
            Assert.AreEqual(credentials.UserIdentifier, "teddymccuddles@gmail.com");
            Assert.AreEqual(credentials.IdentityProvider, "www.google.com");
        }

    }
}
