using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Application.Domain.SiteMap;
using PostaFlya.Domain.Flier.Query;
using Website.Application.Content;
using Website.Application.Schedule;
using Website.Application.Tests.Mocks;
using Website.Application.Util;
using Website.Infrastructure.Configuration;

namespace PostaFlya.Application.Domain.Tests.SiteMap
{
    public class SiteMapXmlGenJobActionTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
             
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {

        }

        [Test]
        public void SiteMapXmlGenJobActionGeneratesSiteMapForAllFliersInSearchServiceTest()
        {
            Kernel.Unbind<TempFileStorageInterface>();
            var tmpStorage = Kernel.GetMock<TempFileStorageInterface>();
            tmpStorage.Setup(filestore => filestore.GetTempPath())
                .Returns(Path.GetTempPath());
            Kernel.Bind<TempFileStorageInterface>().ToConstant(tmpStorage.Object);

            var blobstorage = ApplicationMockUtil.SetupMockBlobStorage(Kernel);

            Kernel.Unbind<ConfigurationServiceInterface>();
            var config = Kernel.GetMock<ConfigurationServiceInterface>();
            config.Setup(serv => serv.GetSetting("SiteUrl")).Returns("http://postaflya.com");
            Kernel.Bind<ConfigurationServiceInterface>().ToConstant(config.Object);

            var flierCount = 1;
            Kernel.Unbind<FlierSearchServiceInterface>();
            var searchService = Kernel.GetMock<FlierSearchServiceInterface>();
            searchService.Setup(src => src.IterateAllIndexedFliers(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                         .Returns<int, int, bool>((mintake, skip, friendly) =>
                             {
                                 IList<string> ret = new List<string>();
                                 for (var i = 0; i < mintake && flierCount <= 60; i++)
                                 {
                                     ret.Add("flierid@1234_" + flierCount++);
                                 }
                                 return ret;
                             });
            Kernel.Bind<FlierSearchServiceInterface>().ToConstant(searchService.Object);

            var sub = Kernel.Get<SiteMapXmlGenJobAction>();

            var jobBase = new JobBase();
            sub.Run(jobBase);

            Assert.True(blobstorage.ContainsKey("sitemap.xml"));
            Assert.True(blobstorage.ContainsKey("sitemap1.xml"));

            Kernel.Unbind<TempFileStorageInterface>();
            Kernel.Unbind<BlobStorageInterface>();
            Kernel.Unbind<ConfigurationServiceInterface>();
            Kernel.Unbind<FlierSearchServiceInterface>();
        }

    }
}
