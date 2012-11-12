using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.Environment;
using PostaFlya.DataRepository.Content;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Website.Domain.Content.Query;
using Website.Domain.Location;

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


        ImageRepositoryInterface _repository;
        ImageQueryServiceInterface _queryService;

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

            _repository = Kernel.Get<ImageRepositoryInterface>();
            _queryService = Kernel.Get<ImageQueryServiceInterface>();
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
            var repository = Kernel.Get<ImageRepositoryInterface>();
            Assert.IsNotNull(repository);
            Assert.That(repository, Is.InstanceOf<AzureImageRepository>());

            var queryService = Kernel.Get<ImageQueryServiceInterface>();
            Assert.IsNotNull(queryService);
            Assert.That(queryService, Is.InstanceOf<AzureImageRepository>());
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
            var exists = _queryService.FindById<Image>(source.Id) != null;
            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository }))
            {
                if (exists)
                {
                    _repository.UpdateEntity<Image>(source.Id, e => e.CopyFieldsFrom(source));
                }
                else
                    _repository.Store(source);
            }
        }

        private ImageInterface Query(ImageInterface source)
        {
            var storedbyid = _queryService.FindById<Image>(source.Id);
            var storedbybrowser = _queryService.GetByBrowserId<Image>(source.BrowserId).FirstOrDefault();

            AssertAreEqual(source, storedbybrowser);
            AssertAreEqual(source, storedbyid);

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
