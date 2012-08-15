using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using WebSite.Application.WebsiteInformation;
using WebSite.Azure.Common.Environment;
using WebSite.Azure.Common.TableStorage;
using WebSite.Application.Azure.WebsiteInformation;

namespace WebSite.Application.Azure.Tests
{
    [TestFixture]
    public class WebsiteInfoServiceAzureTests
    {
        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Row("dev")] 
        [Row("real")]
        public WebsiteInfoServiceAzureTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<TableNameAndPartitionProviderInterface>()
                .ToConstant(new TableNameAndPartitionProvider<SimpleExtendableEntity>()
                            {{typeof (SimpleExtendableEntity), 0, "websiteinfotest", e => "", e => e.Get<string>("url")}})
                .WhenAnyAnchestorNamed("websiteinfo");

            Kernel.Rebind<WebsiteInfoServiceInterface>().To<WebsiteInfoServiceAzure>();

            Reinit();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            AzureEnv.UseRealStorage = false;
        }

        private void Reinit()
        {
            Kernel.Get<AzureTableContext>("websiteinfo").InitFirstTimeUse();
            Kernel.Get<AzureTableContext>("websiteinfo").Delete<SimpleExtendableEntity>(null, 0);
        }

        private void RegisterPostaFlya()
        {
            
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var websiteInfo = new WebsiteInfo()
            {
                Tags = "event,social,comedy,theatre,books,pets,lost,found,services,music,fashion,food & drink,job,task,wanted,for sale,for free,sport,automotive,education,sale,garage,film,art & craft,photography,accommodation,technology,property,kids,politics",
                WebsiteName = "postaFlya",
                BehaivoirTags = "postaFlya",
                FacebookAppID = "facebookappid",
                FacebookAppSecret = "itsasecret"
            };

            websiteInfoService.RegisterWebsite("www.postaFlya.com", websiteInfo);
            
        }

        [Test]        
        public void WebsiteInfoServiceAzureGetWebsiteNameTest()
        {
            RegisterPostaFlya();
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var websiteName = websiteInfoService.GetWebsiteName("www.postaFlya.com");

            Assert.AreEqual(websiteName, "postaFlya"); 

        }

        [Test]
        public void WebsiteInfoServiceAzureRegisterWebsiteTest()
        {
            RegisterPostaFlya();

            var ctx = Kernel.Get<AzureTableContext>("websiteinfo");

            var websites = ctx
                .PerformQuery<SimpleExtendableEntity>()
                .Select(e => e.Get<string>("url"))
                .ToList();

            Assert.Count(1, websites);

        }

        [Test]
        public void WebsiteInfoServiceAzureGetWebsiteTagsTest()
        {
            RegisterPostaFlya();
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var websiteTags = websiteInfoService.GetBehaivourTags("www.postaFlya.com");

            Assert.AreEqual(websiteTags.ToString(), "postaFlya"); 

        }

        [Test]
        public void WebsiteInfoServiceAzureGetWebsiteInfoTest()
        {
            RegisterPostaFlya();
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var websiteInfo = websiteInfoService.GetWebsiteInfo("www.postaFlya.com");

            Assert.AreEqual(websiteInfo.BehaivoirTags.ToString(), "postaFlya");
            Assert.AreEqual(websiteInfo.WebsiteName, "postaFlya");
            Assert.AreEqual(websiteInfo.FacebookAppID, "facebookappid");
            Assert.AreEqual(websiteInfo.FacebookAppSecret, "itsasecret");
            Assert.AreEqual(websiteInfo.Tags.ToString(), "event,social,comedy,theatre,books,pets,lost,found,services,music,fashion,food & drink,job,task,wanted,for sale,for free,sport,automotive,education,sale,garage,film,art & craft,photography,accommodation,technology,property,kids,politics");

        }

        [Test]
        public void WebsiteInfoServiceAzureGetTagsAndTagGroupsTest()
        {
            RegisterPostaFlya();
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var tagsList = websiteInfoService.GetTags("www.postaFlya.com");

            Assert.Count(30, tagsList.Split(new char[] { ',' }));

            Assert.AreEqual("event,social,comedy,theatre,books,pets,lost,found,services,music,fashion,food & drink,job,task,wanted,for sale,for free,sport,automotive,education,sale,garage,film,art & craft,photography,accommodation,technology,property,kids,politics", tagsList.ToString());
        }
    }
}
