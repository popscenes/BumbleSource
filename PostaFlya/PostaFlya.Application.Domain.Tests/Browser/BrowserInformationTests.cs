using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using WebSite.Application.Authentication;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using WebSite.Infrastructure.Authentication;
using PostaFlya.Domain.Browser.Query;
using WebSite.Test.Common;
using PostaFlya.Mocks.Domain.Data;

namespace PostaFlya.Application.Domain.Tests.Browser
{
    [TestFixture]
    public class BrowserInformationTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        //TODO all the principal and mock repository for BrowserInformation
        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<BrowserInformationInterface>().To<BrowserInformation>();
            HttpContextMock.FakeHttpContext(Kernel);
            


        }

        [FixtureTearDown]
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
            var browser = Kernel.Get<BrowserInformationInterface>();
            Assert.IsInstanceOfType<BrowserInformation>(browser);

            Assert.IsNotNull(browser.Browser);
            Assert.IsFalse(string.IsNullOrWhiteSpace(browser.Browser.Id));

            var httpContext = Kernel.Get<HttpContextBase>();

            var httpCookie = httpContext.Response.Cookies["tempId"];
            Assert.IsTrue(httpCookie != null && httpCookie.Value == browser.Browser.Id);
        }

        [Test]
        public void BrowserInformationForNotEmptyPrincipalIsCreated()
        {
            WebIdentity id = new WebIdentity();
            GenericPrincipal prin = new GenericPrincipal(id, new string[] { "Participant" });

            var mockHttpContext = Kernel.GetMock<HttpContextBase>();
            mockHttpContext.Setup(_ => _.User).Returns(prin);

            //add the default sts browser to the repo.
            var repo = Kernel.Get<BrowserRepositoryInterface>();
            repo.Store(Kernel.Get<BrowserInterface>(ctx => ctx.Has("ststestbrowser")));

            var browser = Kernel.Get<BrowserInformationInterface>();
            Assert.IsInstanceOfType<BrowserInformation>(browser);

            var queryinterface = Kernel.Get<BrowserQueryServiceInterface>();
            var principal = Kernel.Get<WebPrincipalInterface>();

            var storedBrowser = queryinterface.FindByIdentityProvider(principal.ToCredential());

            Assert.IsNotNull(browser.Browser);
            Assert.IsFalse(string.IsNullOrWhiteSpace(browser.Browser.Id));

            Kernel.Unbind<WebPrincipalInterface>();
        }
    }
}
