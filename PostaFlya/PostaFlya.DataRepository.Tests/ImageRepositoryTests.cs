using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using Website.Domain.Content;
using Website.Domain.Location;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;
using Website.Test.Common;

namespace PostaFlya.DataRepository.Tests
{
    [TestFixture("dev")]
    //[TestFixture("real")]
    public class ImageRepositoryTests
    {
        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public ImageRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 


        private UnitOfWorkForRepoInterface _unitOfWork;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(new TableNameAndPartitionProvider<ImageInterface>()
//                            {
//                            {typeof(ImageTableEntry), ImageStorageDomain.IdPartition, "imageTest", i => i.Id, i => i.Id},    
//                            {typeof(ImageTableEntry), ImageStorageDomain.BrowserPartition, "imageTest", i => i.BrowserId, i => i.Id}
//                            })
//                .WhenAnyAnchestorNamed("image")
//                .InSingletonScope();
//
//            var context = Kernel.Get<AzureTableContext>("image");
//            context.InitFirstTimeUse();
//            context.Delete<ImageTableEntry>(null, ImageStorageDomain.IdPartition);
//            context.Delete<ImageTableEntry>(null, ImageStorageDomain.BrowserPartition);
//            context.SaveChanges();

            _unitOfWork = Kernel.Get<UnitOfWorkForRepoInterface>();

        }
        
        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            //Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        [Test]
        public void TestCreateImageRepository()
        {
            using (_unitOfWork.Begin())
            {
                var repository = Kernel.Get<GenericRepositoryInterface>();
                Assert.IsNotNull(repository);
                Assert.That(repository, Is.InstanceOf<JsonRepository>());

                var queryService = Kernel.Get<GenericQueryServiceInterface>();
                Assert.IsNotNull(queryService);
                Assert.That(queryService, Is.InstanceOf<JsonRepository>());
            }
        }

        [Test]
        public void StoreImageRepositoryTest()
        {
            StoreImageRepository();
        }

        public ImageInterface StoreImageRepository()
        {
            
            var imgId = Guid.NewGuid().ToString();
            var browsId = Guid.NewGuid().ToString();
            var image = new Image()
                            {
                                Id = imgId,
                                BrowserId = browsId,
                                Title = "YoYoYo",
                                Status = ImageStatus.Processing,
                                Location = new Location(22, 22)
                            };

            Store(image);

            return image; 
        }

        [Test]
        public void QueryImageRepositoryTest()
        {
            QueryImageRepository();
        }

        public ImageInterface QueryImageRepository()
        {
            var img = StoreImageRepository();
            return Query(img);
        }

        [Test]
        public void TestQueryModifySaveImageRepository()
        {
            var image = QueryImageRepository();
            image.Title = "BlahBlahBlah";
            image.Status = ImageStatus.Ready;
            image.Location = new Location(20,20);
            Store(image);
            Query(image);
        }

        private void Store(ImageInterface source)
        {
            var exists = StoreGetUpdate.Get<Image>(source.Id, Kernel) != null;
            if (exists)
                StoreGetUpdate.UpdateOne(source as Image, Kernel, ImageInterfaceExtensions.CopyFieldsFrom); 
            else
            {
                StoreGetUpdate.Store((Image)source, Kernel);
            }
            

        }

        private ImageInterface Query(ImageInterface source)
        {
            var storedbyid = StoreGetUpdate.Get<Image>(source.Id, Kernel);

            using (_unitOfWork.Begin())
            {
                var queryChannel = Kernel.Get<QueryChannelInterface>();
                var storedbybrowser = queryChannel.Query(new GetByBrowserIdQuery<Image>() { BrowserId = source.BrowserId },
                                                         new List<Image>()).FirstOrDefault();

                AssertAreEqual(source, storedbybrowser);
                AssertAreEqual(source, storedbyid);
            }

            return storedbyid;
        }

        private void AssertAreEqual(ImageInterface source, ImageInterface query)
        {
            Assert.IsNotNull(query);
            Assert.AreEqual(source.Id, query.Id);
            Assert.AreEqual(source.Title, query.Title);
            Assert.AreEqual(source.BrowserId, query.BrowserId);
            Assert.AreEqual(source.Status, query.Status);
            Assert.AreEqual(source.Location, query.Location);
        }

    }
}
