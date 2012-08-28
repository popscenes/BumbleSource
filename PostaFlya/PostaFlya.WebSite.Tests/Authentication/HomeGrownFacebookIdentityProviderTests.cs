using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Authentication;
using Website.Infrastructure.Authentication;
using Website.Test.Common;

namespace PostaFlya.Website.Tests.Authentication
{
    [TestFixture]
    public class HomeGrownFacebookIdentityProviderTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            HttpContextMock.FakeHttpContext(Kernel);
        }

        [Test]
        public void HomeGrownFacebookIdentityProviderTestRequestAuth()
        {
            var httpContext = Kernel.GetMock<HttpContextBase>();
            var facebookProvider = new HomeRolledFacebookProvider(httpContext.Object, "testID", "testSecret")
            {
                CallbackUrl = "http://localhost:1805/account/Authenticate"
            };
            facebookProvider.RequestAuthorisation();



            Assert.IsTrue(httpContext.Object.Response.RedirectLocation.Contains("https://www.facebook.com/dialog/oauth"));
        }

        //[Test]
        //public void HomeGrownFacebookIdentityProviderTestgetCredentials()
        //{
        //    FacebookGraph graph = new FacebookGraph()
        //    {
        //        Email = "anthonyborg@hotmail.com",
        //        Id = "12321",
        //        Name = "Anthony Borg",
        //        Link = new Uri("http://www.facebook.com/TeddyMcCuddles")
        //    };

        //    var httpContext = Kernel.GetMock<HttpContextBase>();
            
        //    var facebookProvider = new HomeRolledFacebookProvider(httpContext.Object, "testID", "testSecret")
        //    {
        //        FacebookGraph = graph
        //    };

        //    var identityCredentials = facebookProvider.GetCredentials();
        //    Assert.AreEqual(identityCredentials.Email, "anthonyborg@hotmail.com");
        //    Assert.AreEqual(identityCredentials.Name, "Anthony Borg");
        //    Assert.AreEqual(identityCredentials.UserIdentifier, "12321");
        //    Assert.AreEqual(identityCredentials.IdentityProvider, IdentityProviders.FACEBOOK);
        //}
    }
}
