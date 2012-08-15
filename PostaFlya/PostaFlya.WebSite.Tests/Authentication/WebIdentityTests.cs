using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using WebSite.Application.Authentication;
using WebSite.Infrastructure.Authentication;

namespace PostaFlya.WebSite.Tests.Authentication
{
    [TestFixture]
    public class WebIdentityTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        private FormsAuthenticationTicket _authTicket = null;

        [FixtureSetUp]
        public void FixtureSetUp()
        {

            var identityProviderCredentials = new IdentityProviderCredential
            {
                Email = "teddymccuddles@gmail.com",
                Name = "Anthony Borg",
                UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho",
                IdentityProvider = IdentityProviders.GOOGLE
            };

            _authTicket = new FormsAuthenticationTicket(
                            1, //version
                            identityProviderCredentials.Name, // user name
                            DateTime.Now,             //creation
                            DateTime.Now.AddMinutes(30), //Expiration
                            false, //Persistent
                            identityProviderCredentials.ToString());
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {

        }

        [Test]
        public void WebIdentityTestCreate()
        {
            var webIdentity = new WebIdentity(_authTicket);
            Assert.AreEqual<string>(webIdentity.NameIdentifier, "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho");
            Assert.AreEqual<string>(webIdentity.Name, "Anthony Borg");
            Assert.AreEqual<string>(webIdentity.EmailAddress, "teddymccuddles@gmail.com");
            Assert.AreEqual<string>(webIdentity.IdentityProvider, IdentityProviders.GOOGLE);
            Assert.IsTrue(webIdentity.IsAuthenticated);
        }

        [Test]
        public void WebIdentityTestToCredential()
        {
            var webIdentity = new WebIdentity(_authTicket);
            var identityProviderCredentials = webIdentity.ToCredential();
            Assert.AreEqual<string>(identityProviderCredentials.UserIdentifier, "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho");
            Assert.AreEqual<string>(identityProviderCredentials.IdentityProvider, IdentityProviders.GOOGLE);
            Assert.AreEqual<string>(identityProviderCredentials.Email, "teddymccuddles@gmail.com");
            Assert.AreEqual<string>(identityProviderCredentials.Name, "Anthony Borg");
        }
    }
}
