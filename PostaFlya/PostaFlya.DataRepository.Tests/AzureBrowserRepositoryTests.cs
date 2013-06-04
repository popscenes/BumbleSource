using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;
using Website.Mocks.Domain.Defaults;

namespace PostaFlya.DataRepository.Tests
{
    [TestFixture("dev")]
    //[TestFixture("real")]
    public class AzureBrowserRepositoryTests
    {
        private GenericRepositoryInterface _repository;
        private GenericQueryServiceInterface _queryService;

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

        public BrowserInterface StoreBrowserRepository()
        {
            return BrowserTestData.StoreOne(GetBrowser(), _repository, Kernel);
        }

        [Test]
        public void StoreBrowserNullLocationRepositoryTest()
        {
            StoreBrowserNullLocationRepository();
        }

        public BrowserInterface StoreBrowserNullLocationRepository()
        {
            var browser = GetBrowser();
            return BrowserTestData.StoreOne(browser, _repository, Kernel);
        }

        private Website.Domain.Browser.Browser GetBrowser()
        {
            var externalId = Guid.NewGuid();

            var token = new AccessToken()
            {
                Expires = DateTime.Now.AddDays(1),
                Token = "memememe",
                Permissions = "post"
            };

            var ret = new Website.Domain.Browser.Browser(Guid.NewGuid().ToString())
                       {
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
        public void TestSaveQueryModifySaveBrowserRepository()
        {
            var source = QueryBrowserRepository();
            BrowserInterface entityToStore = null;
        
            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository }))
            {
                _repository.UpdateEntity<Website.Domain.Browser.Browser>(source.Id
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
