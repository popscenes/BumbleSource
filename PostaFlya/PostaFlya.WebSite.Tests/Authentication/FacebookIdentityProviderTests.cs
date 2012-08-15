﻿using System;
using System.Configuration;
using System.IO;
using System.Security.Principal;
using System.Web;
using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using WebSite.Application.Authentication;
using WebSite.Infrastructure.Authentication;
using WebSite.Test.Common;

namespace PostaFlya.WebSite.Tests.Authentication
{
    [TestFixture]
    public class FacebookIdentityProviderTests
    {

        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [FixtureSetUp]
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
