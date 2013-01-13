using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Content;
using Website.Application.SiteMap;
using Website.Application.Tests.Mocks;
using Website.Application.Util;
using Website.Infrastructure.Configuration;

namespace Website.Application.Tests.SiteMap
{
    [TestFixture]
    public class SiteMapIndexBuilderTests
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
        public void SiteMapIndexBuilderSplitsUrlsBy50000Test()
        {

            Kernel.Unbind<TempFileStorageInterface>();
            var tmpStorage = Kernel.GetMock<TempFileStorageInterface>();
            tmpStorage.Setup(filestore => filestore.GetTempPath())
                .Returns(Path.GetTempPath());
            Kernel.Bind<TempFileStorageInterface>().ToConstant(tmpStorage.Object);

            var blobstorage = ApplicationMockUtil.SetupMockBlobStorage(Kernel);

          
            const string siteMapFileFormat = "sitemap{0}.xml";

            const string site = "http://postaflya.com";
            using (var siteMapIndex = new SiteMapIndexBuilder(site, siteMapFileFormat
                , Kernel.Get<TempFileStorageInterface>(), Kernel.Get<BlobStorageInterface>()))
            {
                for (var i = 0; i < 50001; i++)
                {
                    siteMapIndex.AddPath("/" + i);
                }
            }

            Assert.True(blobstorage.ContainsKey("sitemap.xml"));
            Assert.True(blobstorage.ContainsKey("sitemap1.xml"));
            Assert.True(blobstorage.ContainsKey("sitemap2.xml"));

            Kernel.Unbind<TempFileStorageInterface>();
            Kernel.Unbind<BlobStorageInterface>();
            Kernel.Unbind<ConfigurationServiceInterface>();
        }
    }
}
