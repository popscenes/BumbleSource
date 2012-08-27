using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Ninject.MockingKernel.Moq;
using MbUnit.Framework;
using PostaFlya.Application.Domain.ExternalSource;
using WebSite.Infrastructure.Command;
using PostaFlya.Mocks.Domain.Data;
using WebSite.Infrastructure.Authentication;
using WebSite.Application.Tests.Intergrations;
using WebSite.Test.Common.Facebook;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Flier.Command;
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




        [FixtureSetUp]
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
            //     RepoUtil.GetMockStore<FlierInterface>()
            //    , RepoUtil.GetMockStore<CommentInterface>()
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

            Kernel.Bind<UrlContentRetrieverFactoryInterface>().ToConstant(urlRetrieverFactory.Object);
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Facebookutils.TestUserDelete(testFBUser.id);
        }

        [Test]        
        public void FacebookFlierImporterCanImportTest()
        {
            var facebookFlierImporter = new FacebookFlierImporter(Kernel.GetMock<FlierQueryServiceInterface>().Object,
                Kernel.GetMock<UrlContentRetrieverFactoryInterface>().Object,
                Kernel.GetMock<CommandBusInterface>().Object);

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
            var facebookFlierImporter = new FacebookFlierImporter(Kernel.GetMock<FlierQueryServiceInterface>().Object, 
                Kernel.GetMock<UrlContentRetrieverFactoryInterface>().Object,
                Kernel.GetMock<CommandBusInterface>().Object);

            var fliers = facebookFlierImporter.ImportFliers(browser);
            Assert.Count(5, fliers.ToList());
            var fliersList = fliers.ToList();
            var repo = Kernel.GetMock<FlierRepositoryInterface>();
            fliersList[0].Id = "extflier1";
            repo.Object.Store(fliersList[0]);
            fliersList[1].Id = "extflier2";

            repo.Object.Store(fliersList[1]);

            fliers = facebookFlierImporter.ImportFliers(browser);
            Assert.Count(3, fliers.ToList());

            fliersList = fliers.ToList();
            Assert.AreEqual(fliersList[0].ExternalSource, IdentityProviders.FACEBOOK);
            Assert.AreEqual(fliersList[0].Title, "Test Event 1");
            Assert.AreEqual(fliersList[0].Description, "this is a test event yo");
            Assert.AreEqual(fliersList[0].EffectiveDate.ToString("yyyy-MM-dd"), DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"));
            Assert.AreEqual(fliersList[0].Location.Description, "After The Tears");
            Assert.IsTrue(Math.Abs(fliersList[0].Location.Latitude - -37.8839340209961) < 0.0001);
            Assert.IsTrue(Math.Abs(fliersList[0].Location.Longitude - 145.0004344094) < 0.0001);
            Assert.AreEqual(fliersList[0].Status, FlierStatus.Pending);
            Assert.IsTrue(fliersList[0].Image.HasValue);
        }
    }
}
