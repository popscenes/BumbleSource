﻿using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Application.Domain.ExternalSource;
using Website.Domain.Browser.Query;
using Website.Domain.Location;
using Website.Infrastructure.Command;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using Website.Test.Common;
using Website.Test.Common.Facebook;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Application.Domain.Content;
using Website.Mocks.Domain.Data;

namespace PostaFlya.Application.Domain.Tests.ExternalSource
{
    [TestFixture]    
    public class FacebookFlierImporterTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        private Website.Domain.Browser.Browser browser;
        private AccessToken validToken;
        private AccessToken invalidTimeToken;
        private AccessToken invalidPermissionsToken;

        protected FacebookTestUser testFBUser;




        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            browser = BrowserTestData.GetOne(Kernel) as Website.Domain.Browser.Browser;
            Facebookutils.TestUserAdd("Heavy Metal Kid", "user_events,friends_events,publish_stream,create_event");
            testFBUser = Facebookutils.TestUserGet();

            validToken = new AccessToken()
            {
                Expires = DateTime.Now.AddDays(1),
                Permissions = "installed,user_Events",
                Token = testFBUser.access_token
            };

            invalidTimeToken = new AccessToken()
            {
                Expires = DateTime.Now.AddDays(-1),
                Permissions = "installed,user_Events",
                Token = testFBUser.access_token
            };

            invalidPermissionsToken = new AccessToken()
            {
                Expires = DateTime.Now.AddDays(1),
                Permissions = "installed",
                Token = testFBUser.access_token
            };

            //TestRepositoriesNinjectModule.SetUpFlierRepositoryAndQueryService(Kernel,
            //     RepoCoreUtil.GetMockStore<FlierInterface>()
            //    , RepoCoreUtil.GetMockStore<CommentInterface>()
            //    , new List<LikeInterface>());

            

            Facebookutils.TestEventAdd(testFBUser.access_token);
            Facebookutils.TestEventAdd(testFBUser.access_token);
            Facebookutils.TestEventAdd(testFBUser.access_token);
            Facebookutils.TestEventAdd(testFBUser.access_token);
            Facebookutils.TestEventAdd(testFBUser.access_token);

            var urlRetrieverFactory = Kernel.GetMock<UrlContentRetrieverFactoryInterface>();
            var urlImageRetriever = Kernel.GetMock<UrlContentRetrieverInterface>();
            var content = new Website.Domain.Content.Content()
                              {
                                  Data = new byte[100],
                                  Type = Website.Domain.Content.Content.ContentType.Image
                              };
            urlImageRetriever.Setup(_ => _.GetContent(It.IsAny<string>())).Returns(content);

            urlRetrieverFactory.Setup(_ => _.GetRetriever(It.IsAny<Website.Domain.Content.Content.ContentType>())).
                Returns(urlImageRetriever.Object);

            Kernel.Rebind<UrlContentRetrieverFactoryInterface>().ToConstant(urlRetrieverFactory.Object);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Facebookutils.TestUserDelete(testFBUser.id);
        }

        [Test]        
        public void FacebookFlierImporterCanImportTest()
        {
            var facebookFlierImporter = new FacebookFlierImporter(
                Kernel.Get<QueryChannelInterface>(),
                Kernel.GetMock<GenericQueryServiceInterface>().Object,
                Kernel.GetMock<UrlContentRetrieverFactoryInterface>().Object,
                Kernel.GetMock<MessageBusInterface>().Object);

            browser.ExternalCredentials.First(_ => _.IdentityProvider == IdentityProviders.FACEBOOK).AccessToken = validToken;
            Assert.IsTrue(facebookFlierImporter.CanImport(browser));

            browser.ExternalCredentials.First(_ => _.IdentityProvider == IdentityProviders.FACEBOOK).AccessToken = invalidTimeToken;
            Assert.IsFalse(facebookFlierImporter.CanImport(browser));

            browser.ExternalCredentials.First(_ => _.IdentityProvider == IdentityProviders.FACEBOOK).AccessToken = invalidPermissionsToken;
            Assert.IsFalse(facebookFlierImporter.CanImport(browser));
        }

        [Test]
        public void FacebookFlierImporterImportFliersTest()
        {
            browser.ExternalCredentials.First(_ => _.IdentityProvider == IdentityProviders.FACEBOOK).AccessToken = validToken;
            var facebookFlierImporter = new FacebookFlierImporter(
                Kernel.Get<QueryChannelInterface>(),
                Kernel.GetMock<GenericQueryServiceInterface>().Object, 
                Kernel.GetMock<UrlContentRetrieverFactoryInterface>().Object,
                Kernel.GetMock<MessageBusInterface>().Object);

            var fliers = facebookFlierImporter.ImportFliers(browser);
            AssertUtil.Count(5, fliers.ToList());
            var fliersList = fliers.ToList();
            var repo = Kernel.GetMock<GenericRepositoryInterface>();
            fliersList[0].Id = "extflier1";
            new Location();
            repo.Object.Store(fliersList[0]);
            fliersList[1].Id = "extflier2";
            new Location();
            repo.Object.Store(fliersList[1]);

            fliers = facebookFlierImporter.ImportFliers(browser);
            AssertUtil.Count(3, fliers.ToList());

            fliersList = fliers.ToList();
            Assert.AreEqual(fliersList[0].ExternalSource, IdentityProviders.FACEBOOK);
            Assert.AreEqual(fliersList[0].Title, "Test Event 1");
            Assert.AreEqual(fliersList[0].Description, "this is a test event yo");
            Assert.AreEqual(fliersList[0].EffectiveDate.ToString("yyyy-MM-dd"), DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"));
            Assert.AreEqual(fliersList[0].Status, FlierStatus.Pending);
            Assert.IsTrue(fliersList[0].Image.HasValue);
        }
    }
}
