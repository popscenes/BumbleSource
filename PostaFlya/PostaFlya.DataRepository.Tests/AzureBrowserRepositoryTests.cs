using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using PostaFlya.Domain.Browser.Event;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;
using PostaFlya.Mocks.Domain.Data;
using Website.Mocks.Domain.Defaults;
using Browser = PostaFlya.Domain.Browser.Browser;
using BrowserInterface = PostaFlya.Domain.Browser.BrowserInterface;

namespace PostaFlya.DataRepository.Tests
{
    [TestFixture("dev")]
    //[TestFixture("real")]
    public class AzureBrowserRepositoryTests
    {
        private GenericRepositoryInterface _repository;
        private GenericQueryServiceInterface _queryService;
        private QueryChannelInterface _queryChannel;

        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public AzureBrowserRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
//            var browserTable = new TableNameAndPartitionProvider<BrowserInterface>()
//                                   {
//                                       {typeof(BrowserTableEntry), BrowserStorageDomain.IdPartition, "browserTest", i => i.Id, i => i.Id},
//                                       {typeof(BrowserTableEntry), BrowserStorageDomain.HandlePartition, "browserTest", i => i.Handle, i => i.Id}
//                                   };
//            var browserCredsTable = new TableNameAndPartitionProvider<BrowserIdentityProviderCredential>()
//                                              {
//                                                  {typeof(BrowserCredentialsTableEntry), BrowserIdentityProviderCredential.IdPartition, "browserCredsTest", i => i.Id, i => i.Id},
//                                                  {typeof(BrowserCredentialsTableEntry), BrowserIdentityProviderCredential.HashPartition, "browserCredsTest", i => i.GetHash(), i => i.Id},    
//                                              };
//
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//            .ToConstant(new TableNameAndPartitionProviderCollection()
//                                {
//                                    browserTable,
//                                    browserCredsTable
//                                })
//            .WhenAnyAnchestorNamed("browser")
//            .InSingletonScope();

//            var context = Kernel.Get<AzureTableContext>("browser");
//            context.InitFirstTimeUse();
//            context.Delete<BrowserTableEntry>(null, BrowserStorageDomain.IdPartition);
//            context.Delete<BrowserTableEntry>(null, BrowserStorageDomain.HandlePartition);
//            context.Delete<BrowserCredentialsTableEntry>(null, BrowserIdentityProviderCredential.IdPartition);
//            context.Delete<BrowserCredentialsTableEntry>(null, BrowserIdentityProviderCredential.HashPartition);
//            context.SaveChanges();

            _repository = Kernel.Get<GenericRepositoryInterface>();
            _queryService = Kernel.Get<GenericQueryServiceInterface>();
            _queryChannel = Kernel.Get<QueryChannelInterface>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            //Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        [Test]
        public void TestCreateBrowserRepository()
        {
            var browserRepository = Kernel.Get<GenericRepositoryInterface>();
            Assert.IsNotNull(browserRepository);
            Assert.That(browserRepository, Is.InstanceOf<JsonRepository>());

            var browserQuery = Kernel.Get<GenericQueryServiceInterface>();
            Assert.IsNotNull(browserQuery);
            Assert.That(browserQuery, Is.InstanceOf<JsonRepository>());
        }

        [Test]
        public void StoreBrowserRepositoryTest()
        {
            StoreBrowserRepository();
        }

        [Test]
        public void FindByFriendlyIdForBrowserAggreagateReturnsBrowser()
        {
            var brows = StoreBrowserRepository();
            var res = _queryChannel.Query(new FindByFriendlyIdQuery() {FriendlyId = brows.FriendlyId}, (Browser) null);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Id, Is.EqualTo(brows.Id));
        }

        public BrowserInterface StoreBrowserRepository()
        {
            var ret = BrowserTestData.StoreOne(GetBrowser(), _repository, Kernel, false);
            var indexers = Kernel.GetAll<HandleEventInterface<BrowserModifiedEvent>>();
            foreach (var handleEvent in indexers)
            {
                handleEvent.Handle(new BrowserModifiedEvent() {NewState = (Browser) ret});
            }
            return ret;
        }

        [Test]
        public void StoreBrowserNullLocationRepositoryTest()
        {
            StoreBrowserNullLocationRepository();
        }

        public BrowserInterface StoreBrowserNullLocationRepository()
        {
            var browser = GetBrowser();
            return BrowserTestData.StoreOne(browser, _repository, Kernel, false);
        }

        private Browser GetBrowser()
        {
            var externalId = Guid.NewGuid();

            var token = new AccessToken()
            {
                Expires = DateTime.Now.AddDays(1),
                Token = "memememe",
                Permissions = "post"
            };

            var ret = new Browser()
                       {
                           Id = Guid.NewGuid().ToString(),
                           FriendlyId = "YoYo",
                           Roles = new Roles { "SomeRole" },
                           FirstName = "FirstName",
                           Surname = "Surname",
                           MiddleNames = "MiddleNames",
                           EmailAddress = "test@test.com",
                           AvatarImageId = GlobalDefaultsNinjectModule.DefaultImageId,
                           Address = GlobalDefaultsNinjectModule.DefaultLocation,
                       };

            ret.ExternalCredentials = new HashSet<BrowserIdentityProviderCredential>()
                                          {
                                              new BrowserIdentityProviderCredential()
                                                  {
                                                      BrowserId = ret.Id,
                                                      IdentityProvider = IdentityProviders.GOOGLE,
                                                      UserIdentifier = "G" + externalId,
                                                      AccessToken = token
                                                  },
                                              new BrowserIdentityProviderCredential()
                                                  {
                                                      BrowserId = ret.Id,
                                                      IdentityProvider = IdentityProviders.FACEBOOK,
                                                      UserIdentifier = "F" + externalId,
                                                      AccessToken = token
                                                  }
                                          };
            return ret;
        }

        [Test]
        public void QueryBrowserRepositoryTest()
        {
            QueryBrowserRepository();
        }

        public BrowserInterface QueryBrowserRepository()
        {
            var storedBrowser = StoreBrowserRepository();
            return BrowserTestData.AssertGetById(storedBrowser, _queryService);
        }

        [Test]
        public void FindByIdentityProviderReturnsBrowser()
        {
            var brows = StoreBrowserRepository();
            var res = _queryChannel.Query(new FindBrowserByIdentityProviderQuery() { Credential = brows.ExternalCredentials.First()}, (Browser)null);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Id, Is.EqualTo(brows.Id));
        }

        [Test]
        public void TestSaveQueryModifySaveBrowserRepository()
        {
            var source = QueryBrowserRepository();
            BrowserInterface entityToStore = null;
        
            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository }))
            {
                _repository.UpdateEntity<Browser>(source.Id
                    , browser =>
                          {
                              entityToStore = browser;

                              var creds = browser.ExternalCredentials.GetEnumerator();
                              creds.MoveNext();
                              creds.Current.UserIdentifier += "Modified";
                              browser.ExternalCredentials.RemoveWhere(c => !c.UserIdentifier.Contains("Modified"));

                              browser.EmailAddress += ".au";

                              browser.Roles.Add("ANewRole");

                              browser.MiddleNames = "Sergio";

                              browser.Surname += "Modified";

                              browser.AvatarImageId = Guid.NewGuid().ToString();
                          }           
                );
            }

           BrowserTestData.AssertGetById(entityToStore, _queryService);
        }
    }


}
