using System.Linq;
using NUnit.Framework;
using Ninject;
using Website.Application.Azure.WebsiteInformation;
using Website.Application.WebsiteInformation;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Test.Common;

namespace Website.Application.Azure.Tests
{
    [TestFixture("dev")]
//    [TestFixture("real")]
    public class WebsiteInfoServiceAzureTests
    {
        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public WebsiteInfoServiceAzureTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        }

        private string _tableName;
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
//            Kernel.Rebind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(new TableNameAndPartitionProvider<SimpleExtendableEntity>()
//                            {{typeof (SimpleExtendableEntity), 0, "websiteinfotest", e => "", e => e.Get<string>("url")}})
//                .WhenAnyAnchestorNamed("websiteinfo");

            Kernel.Rebind<WebsiteInfoServiceInterface>().To<WebsiteInfoServiceAzure>();

            _tableName =
                Kernel.Get<TableNameAndPartitionProviderServiceInterface>().GetTableName<WebsiteInfoEntity>();

            Reinit();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            AzureEnv.UseRealStorage = false;
        }

        private void Reinit()
        {
            //Kernel.Get<AzureTableContext>("websiteinfo").InitFirstTimeUse();
            var context = Kernel.Get<TableContextInterface>();
            context.Delete<WebsiteInfoEntity>(_tableName, null);
            context.SaveChanges();
        }

        private void RegisterPopscenes()
        {
            
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var websiteInfo = new WebsiteInfo()
            {
                Tags = "event,social,comedy,theatre,books,pets,lost,found,services,music,fashion,food & drink,job,task,wanted,for sale,for free,sport,automotive,education,sale,garage,film,art & craft,photography,accommodation,technology,property,kids,community",
                WebsiteName = "Popscenes",
                BehaivoirTags = "Popscenes",
                FacebookAppID = "facebookappid",
                FacebookAppSecret = "itsasecret",
                PaypalUserId = "paypalId",
                PaypalPassword = "paypalPassword",
                PaypalSignitures = "paypalSigniture",
            };

            websiteInfoService.RegisterWebsite("www.popscenes.com", websiteInfo, true);
            
        }

        [Test]        
        public void WebsiteInfoServiceAzureGetWebsiteNameTest()
        {
            RegisterPopscenes();
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var websiteName = websiteInfoService.GetWebsiteName("www.popscenes.com");

            Assert.AreEqual(websiteName, "Popscenes"); 

        }

        [Test]
        public void WebsiteInfoServiceAzureGetsDefaultSiteIfNoneExist()
        {
            RegisterPopscenes();
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var websiteName = websiteInfoService.GetWebsiteName("www.blah.com");
            var info = websiteInfoService.GetWebsiteInfo("www.blah.com");

            Assert.That(info, Is.Not.Null);
            Assert.AreEqual(websiteName, "Popscenes");
            Assert.AreEqual(websiteName, "Popscenes");

        }

        [Test]
        public void WebsiteInfoServiceAzureRegisterWebsiteTest()
        {
            RegisterPopscenes();

            var ctx = Kernel.Get<TableContextInterface>();

            var websites = ctx
                .PerformQuery<WebsiteInfoEntity>(_tableName)
                .Select(e => e.Get<string>("url"))
                .ToList();

            AssertUtil.Count(1, websites);

        }

        [Test]
        public void WebsiteInfoServiceAzureGetWebsiteTagsTest()
        {
            RegisterPopscenes();
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var websiteTags = websiteInfoService.GetBehaivourTags("www.popscenes.com");

            Assert.AreEqual(websiteTags.ToString(), "Popscenes"); 

        }

        [Test]
        public void WebsiteInfoServiceAzureGetWebsiteInfoTest()
        {
            RegisterPopscenes();
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var websiteInfo = websiteInfoService.GetWebsiteInfo("www.popscenes.com");

            Assert.AreEqual(websiteInfo.BehaivoirTags.ToString(), "Popscenes");
            Assert.AreEqual(websiteInfo.WebsiteName, "Popscenes");
            Assert.AreEqual(websiteInfo.FacebookAppID, "facebookappid");
            Assert.AreEqual(websiteInfo.FacebookAppSecret, "itsasecret");
            Assert.AreEqual(websiteInfo.Tags.ToString(), "event,social,comedy,theatre,books,pets,lost,found,services,music,fashion,food & drink,job,task,wanted,for sale,for free,sport,automotive,education,sale,garage,film,art & craft,photography,accommodation,technology,property,kids,community");

            Assert.AreEqual(websiteInfo.PaypalUserId, "paypalId");
            Assert.AreEqual(websiteInfo.PaypalSignitures, "paypalSigniture");
            Assert.AreEqual(websiteInfo.PaypalPassword, "paypalPassword");

        }

        [Test]
        public void WebsiteInfoServiceAzureGetTagsAndTagGroupsTest()
        {
            RegisterPopscenes();
            var websiteInfoService = Kernel.Get<WebsiteInfoServiceInterface>();
            var tagsList = websiteInfoService.GetTags("www.popscenes.com");

            Assert.That(tagsList.Split(new[] { ',' }).Count(), Is.EqualTo(30));

            Assert.AreEqual("event,social,comedy,theatre,books,pets,lost,found,services,music,fashion,food & drink,job,task,wanted,for sale,for free,sport,automotive,education,sale,garage,film,art & craft,photography,accommodation,technology,property,kids,community", tagsList.ToString());
        }
    }
}
