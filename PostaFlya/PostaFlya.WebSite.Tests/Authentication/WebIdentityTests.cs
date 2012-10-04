using System;
using System.Web.Security;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Authentication;
using Website.Infrastructure.Authentication;

namespace PostaFlya.Website.Tests.Authentication
{
    [TestFixture]
    public class WebIdentityTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        private FormsAuthenticationTicket _authTicket = null;

        [TestFixtureSetUp]
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

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {

        }

        [Test]
        public void WebIdentityTestCreate()
        {
            var webIdentity = new WebIdentity(_authTicket);
            Assert.AreEqual(webIdentity.NameIdentifier, "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho");
            Assert.AreEqual(webIdentity.Name, "Anthony Borg");
            Assert.AreEqual(webIdentity.EmailAddress, "teddymccuddles@gmail.com");
            Assert.AreEqual(webIdentity.IdentityProvider, IdentityProviders.GOOGLE);
            Assert.IsTrue(webIdentity.IsAuthenticated);
        }

        [Test]
        public void WebIdentityTestToCredential()
        {
            var webIdentity = new WebIdentity(_authTicket);
            var identityProviderCredentials = webIdentity.ToCredential();
            Assert.AreEqual(identityProviderCredentials.UserIdentifier, "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho");
            Assert.AreEqual(identityProviderCredentials.IdentityProvider, IdentityProviders.GOOGLE);
            Assert.AreEqual(identityProviderCredentials.Email, "teddymccuddles@gmail.com");
            Assert.AreEqual(identityProviderCredentials.Name, "Anthony Borg");
        }
    }
}
