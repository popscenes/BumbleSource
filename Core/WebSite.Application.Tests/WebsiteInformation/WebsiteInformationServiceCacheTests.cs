using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Moq;
using Website.Application.WebsiteInformation;
using Website.Test.Common;


namespace Website.Application.Tests.WebsiteInformation
{
    [TestFixture]    
    public class WebsiteInformationServiceCacheTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            

            //Kernel.Rebind<WebsiteInfoServiceInterface>().To<WebsiteInfoServiceAzure>();
            //TestRepositoriesinjectModule

            Kernel.Unbind<WebsiteInfoServiceInterface>();
            var websiteInfo = Kernel.GetMock<WebsiteInfoServiceInterface>();



            websiteInfo.Setup(_ => _.GetBehaivourTags(It.IsAny<String>())).Returns("postaFlya");
            websiteInfo.Setup(_ => _.GetWebsiteName(It.IsAny<String>())).Returns("postaFlya");
            var tags = "event,social,comedy,theatre,books,pets,lost,found,services,music,fashion,food & drink,job,task,wanted,for sale,for free,sport,automotive,education,sale,garage,film,art & craft,photography,accommodation,technology,property,kids,politics";
            websiteInfo.Setup(_ => _.GetTags(It.IsAny<String>())).Returns(tags);
            //Kernel.Rebind<WebsiteInfoServiceInterface>().To<WebsiteInfoServiceInterface>(websiteInfo);
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void WebsiteInformationServiceCache()
        {
            var memoryCache = Test.Common.TestUtil.GetMemoryCache();
            WebsiteInformationServiceCache(Kernel, memoryCache);
            memoryCache.Dispose();

            
            var serializeCache = TestUtil.GetSerializingCache();
            WebsiteInformationServiceCache(Kernel, serializeCache);
        }

        public static void WebsiteInformationServiceCache(MoqMockingKernel kernel, ObjectCache memoryCache)
        {
            
            //var websiteInfoService = kernel.Get<WebsiteInfoServiceInterface>();
            var websiteInfoService = kernel.GetMock<WebsiteInfoServiceInterface>();

            //var websiteInfo = new WebsiteInfo()
            //{
            //    Tags = "event,social,comedy,theatre,books,pets,lost,found,services,music,fashion,food & drink,job,task,wanted,for sale,for free,sport,automotive,education,sale,garage,film,art & craft,photography,accommodation,technology,property,kids,politics",
            //    WebsiteName = "postaFlya",
            //    BehaivoirTags = "postaFlya",
            //    FacebookAppID = "facebookappid",
            //    FacebookAppSecret = "itsasecret"
            //};

            //websiteInfoService.RegisterWebsite("www.postaFlya.com", websiteInfo);


            WebsiteInfoServiceInterface cachedInfoService = new CachedWebsiteInfoService(websiteInfoService.Object, memoryCache);

            Assert.Count(0, memoryCache);
            var websiteName = cachedInfoService.GetWebsiteName("www.postaFlya.com");

            Assert.AreEqual(websiteName, "postaFlya");

            websiteName = cachedInfoService.GetWebsiteName("www.postaFlya.com");
            Assert.Count(1, memoryCache);

            var tagsList = cachedInfoService.GetTags("www.postaFlya.com");


            tagsList = cachedInfoService.GetTags("www.postaFlya.com");


            Assert.Count(2, memoryCache);
            Assert.Count(30, tagsList.Split(new char[]{','}));

            var websiteTags = cachedInfoService.GetBehaivourTags ("www.postaFlya.com");
            websiteTags = cachedInfoService.GetBehaivourTags("www.postaFlya.com");

            Assert.AreEqual(websiteTags.ToString(), "postaFlya");
            Assert.Count(3, memoryCache);
        }
    }
}
