using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Authentication;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Test.Common;
using Website.Application.Domain.Browser;
using Website.Domain.Browser;

namespace Website.Application.Domain.Tests.Browser
{
    [TestFixture]
    public class BrowserInformationTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        //TODO all the principal and mock repository for BrowserInformation
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<BrowserInformationInterface>().To<BrowserInformation>();
            HttpContextMock.FakeHttpContext(Kernel);
            


        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<BrowserInformationInterface>();
        }

        [Test]
        public void BrowserInformationForEmptyPrincipalIsCreated()
        {

            IdentityProviderCredential identityProviderCredentials = new IdentityProviderCredential()
                                                                         {
                                                                            Email = "teddymccuddles@gmail.com",
                                                                            IdentityProvider = IdentityProviders.GOOGLE,
                                                                            Name = "anthony borg",
                                                                            UserIdentifier = "fdskljflksdjfslkdfjsdlkjf"
                                                                         };
            FormsAuthenticationTicket authTicket = new
                            FormsAuthenticationTicket(1, //version
                            identityProviderCredentials.Name, // user name
                            DateTime.Now,             //creation
                            DateTime.Now.AddMinutes(30), //Expiration
                            false, //Persistent
                            identityProviderCredentials.ToString());

            WebIdentity id = new WebIdentity();
            GenericPrincipal prin = new GenericPrincipal(id, new string[] { "Participant" });

            var mockHttpContext = Kernel.GetMock<HttpContextBase>();
            mockHttpContext.Setup(_ => _.User).Returns(prin);

            Kernel.Unbind<WebPrincipalInterface>();
            var browser = ResolutionExtensions.Get<BrowserInformationInterface>(Kernel);
            Assert.That(browser, Is.InstanceOf<BrowserInformation>());

            Assert.IsNotNull(browser.Browser);
            Assert.IsFalse(string.IsNullOrWhiteSpace(browser.Browser.Id));

            var httpContext = Kernel.Get<HttpContextBase>();

            var httpCookie = httpContext.Response.Cookies["tempId"];
            Assert.IsTrue(httpCookie != null && httpCookie.Value == browser.Browser.Id);
        }

        [Test]
        public void BrowserInformationForNotEmptyPrincipalIsCreated()
        {
            var id = new WebIdentity();
            var prin = new GenericPrincipal(id, new string[] { "Participant" });

            var mockHttpContext = Kernel.GetMock<HttpContextBase>();
            mockHttpContext.Setup(_ => _.User).Returns(prin);

            //add the default sts browser to the repo.
            var repo = Kernel.Get<GenericRepositoryInterface>();
            repo.Store(Kernel.Get<BrowserInterface>(ctx => ctx.Has("ststestbrowser")));

            var browser = Kernel.Get<BrowserInformationInterface>();
            Assert.That(browser, Is.InstanceOf<BrowserInformation>());

            var queryinterface = Kernel.Get<GenericQueryServiceInterface>();
            var principal = Kernel.Get<WebPrincipalInterface>();

            var storedBrowser = queryinterface.FindBrowserByIdentityProvider(principal.ToCredential());

            Assert.IsNotNull(browser.Browser);
            Assert.IsFalse(string.IsNullOrWhiteSpace(browser.Browser.Id));

            Kernel.Unbind<WebPrincipalInterface>();
        }
    }
}
