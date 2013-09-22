using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;
using PostaFlya.Mocks.Domain.Data;
using Website.Mocks.Domain.Defaults;
using Website.Test.Common;
using Browser = PostaFlya.Domain.Browser.Browser;
using BrowserInterface = PostaFlya.Domain.Browser.BrowserInterface;

namespace PostaFlya.DataRepository.Tests
{
    [TestFixture("dev")]
    //[TestFixture("real")]
    public class AzureBrowserRepositoryTests
    {
        private QueryChannelInterface _queryChannel;
        private UnitOfWorkForRepoInterface _unitOfWorkForRepo;

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

            _unitOfWorkForRepo = Kernel.Get<UnitOfWorkForRepoInterface>();
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
            using (_unitOfWorkForRepo.Begin())
            {
                var browserRepository = Kernel.Get<GenericRepositoryInterface>();
                Assert.IsNotNull(browserRepository);
                Assert.That(browserRepository, Is.InstanceOf<JsonRepository>());

                var browserQuery = Kernel.Get<GenericQueryServiceInterface>();
                Assert.IsNotNull(browserQuery);
                Assert.That(browserQuery, Is.InstanceOf<JsonRepository>()); 
            }
        }

        [Test]
        public void StoreBrowserRepositoryTest()
        {
            StoreBrowserRepository();
        }

        [Test]
        public void FindByFriendlyIdForBrowserAggreagateReturnsBrowser()
        {
            using (_unitOfWorkForRepo.Begin())
            {
                var brows = StoreBrowserRepository();
                var res = _queryChannel.Query(new FindByFriendlyIdQuery<Browser>() {FriendlyId = brows.FriendlyId},
                                              (Browser) null);
                Assert.That(res, Is.Not.Null);
                Assert.That(res.Id, Is.EqualTo(brows.Id));
            }
        }

        public BrowserInterface StoreBrowserRepository()
        {
            var browser = GetBrowser();
            StoreGetUpdate.Store(browser, Kernel);
            return browser;
        }

        [Test]
        public void StoreBrowserNullLocationRepositoryTest()
        {

            StoreBrowserNullLocationRepository();
            
        }

        public BrowserInterface StoreBrowserNullLocationRepository()
        {
            var browser = GetBrowser();
            StoreGetUpdate.Store(browser, Kernel);
            return browser;
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

            var retrievedBrowser = StoreGetUpdate.Get<Browser>(storedBrowser.Id, Kernel);
            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);
            return retrievedBrowser;
        }

        [Test]
        public void FindByIdentityProviderReturnsBrowser()
        {
            using (_unitOfWorkForRepo.Begin())
            {
                var brows = StoreBrowserRepository();
                var res = _queryChannel.Query(new FindBrowserByIdentityProviderQuery() { Credential = brows.ExternalCredentials.First() }, (Browser)null);
                Assert.That(res, Is.Not.Null);
                Assert.That(res.Id, Is.EqualTo(brows.Id));
            }

        }

        [Test]
        public void TestSaveQueryModifySaveBrowserRepository()
        {
            var source = QueryBrowserRepository();
            BrowserInterface entityToStore = null;
       
            StoreGetUpdate.UpdateOne<Browser>(source as Browser, Kernel
                , (browser, sourceBrow) =>  
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
            

            var retrievedBrowser = StoreGetUpdate.Get<Browser>(source.Id, Kernel);
            BrowserTestData.AssertStoreRetrieve(entityToStore, retrievedBrowser);
        }
    }


}
