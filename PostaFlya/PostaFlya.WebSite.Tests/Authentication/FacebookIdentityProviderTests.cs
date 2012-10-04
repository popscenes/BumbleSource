using System.Web;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Authentication;
using Website.Test.Common;

namespace PostaFlya.Website.Tests.Authentication
{
    [TestFixture]
    public class FacebookIdentityProviderTests
    {

        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            HttpContextMock.HttpContextCurrentCreate("http://localhost/", "");
        }

        

        [Test]
        public void FacebookIdentityProviderTestRequestAuth()
        {
            var facebookProvider = new FacebookProvider("testID", "testSecret")
                                       {
                                           CallbackUrl = "http://localhost:1805/account/Authenticate"
                                       };
            facebookProvider.RequestAuthorisation();



            Assert.IsTrue(HttpContext.Current.Response.RedirectLocation.Contains("https://www.facebook.com/dialog/oauth"));
        }

        //[Test]
        //public void FacebookIdentityProviderTestgetCredentials()
        //{
        //    FacebookGraph graph = new FacebookGraph()
        //                              {
        //                                  Email = "anthonyborg@hotmail.com",
        //                                  Id = "12321",
        //                                  Name = "Anthony Borg",
        //                                  Link = new Uri("http://www.facebook.com/TeddyMcCuddles")
        //                              };

        //    var facebookProvider = new FacebookProvider("testID", "testSecret")
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
