using System;
using System.Linq;
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
            Kernel.Bind<BrowserInformationInterface>().To<BrowserInformation>().InTransientScope();
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

            var id = new WebIdentity();
            var prin = new GenericPrincipal(id, new string[] { "Participant" });

            var mockHttpContext = Kernel.GetMock<HttpContextBase>();
            mockHttpContext.Setup(_ => _.User).Returns(prin);

            Kernel.Unbind<WebPrincipalInterface>();
            var browser = Kernel.Get<BrowserInformationInterface>();
            Assert.That(browser, Is.InstanceOf<BrowserInformation>());

            Assert.IsNotNull(browser.Browser);
            Assert.IsFalse(string.IsNullOrWhiteSpace(browser.Browser.Id));

            var httpContext = Kernel.Get<HttpContextBase>();

            var httpCookie = httpContext.Response.Cookies[BrowserInformation.BrowserCookieId];
            Assert.IsTrue(httpCookie != null);
            Assert.IsNotNullOrEmpty(httpCookie.Values[BrowserInformation.TempBrowserId]);

            httpContext.Request.Cookies.Add(httpCookie);

            var browserRet = Kernel.Get<BrowserInformationInterface>();
            Assert.IsNotNull(browserRet.Browser);
            Assert.That(browserRet.Browser.Id, Is.EqualTo(browser.Browser.Id));
        }

        [Test]
        public void BrowserInformationForNotEmptyPrincipalIsCreated()
        {
            //add the default sts browser to the repo.
            var repo = Kernel.Get<GenericRepositoryInterface>();
            var brows = Kernel.Get<BrowserInterface>(ctx => ctx.Has("ststestbrowser"));
            repo.Store(brows);

            var identityProviderCredentials = brows.ExternalCredentials.FirstOrDefault();
            var authTicket = new
                            FormsAuthenticationTicket(1, //version
                            identityProviderCredentials.Name, // user name
                            DateTime.Now,             //creation
                            DateTime.Now.AddMinutes(30), //Expiration
                            false, //Persistent
                            identityProviderCredentials.ToString());


            var id = new WebIdentity(authTicket);
            var prin = new GenericPrincipal(id, new string[] { "Participant" });

            var mockHttpContext = Kernel.GetMock<HttpContextBase>();
            mockHttpContext.Setup(_ => _.User).Returns(prin);

            var browser = Kernel.Get<BrowserInformationInterface>();
            Assert.That(browser, Is.InstanceOf<BrowserInformation>());

            Assert.That(browser.Browser.Id, Is.EqualTo(brows.Id));

            Kernel.Unbind<WebPrincipalInterface>();
        }
    }
}
