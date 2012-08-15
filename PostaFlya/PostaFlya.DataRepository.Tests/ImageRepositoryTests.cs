﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using WebSite.Azure.Common.Environment;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Content;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using PostaFlya.Domain.Content.Query;
using PostaFlya.Domain.Location;
using WebSite.Infrastructure.Command;

namespace PostaFlya.DataRepository.Tests
{
    [TestFixture]
    public class ImageRepositoryTests
    {
        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Row("dev")] 
        [Row("real")]
        public ImageRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 


        ImageRepositoryInterface _repository;
        ImageQueryServiceInterface _queryService;

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<TableNameAndPartitionProviderInterface>()
                .ToConstant(new TableNameAndPartitionProvider<ImageInterface>()
                            {
                            {typeof(ImageTableEntry), ImageStorageDomain.IdPartition, "imageTest", i => i.Id, i => i.Id},    
                            {typeof(ImageTableEntry), ImageStorageDomain.BrowserPartition, "imageTest", i => i.BrowserId, i => i.Id}
                            })
                .WhenAnyAnchestorNamed("image")
                .InSingletonScope();

            var context = Kernel.Get<AzureTableContext>("image");
            context.InitFirstTimeUse();
            context.Delete<ImageTableEntry>(null, ImageStorageDomain.IdPartition);
            context.Delete<ImageTableEntry>(null, ImageStorageDomain.BrowserPartition);
            context.SaveChanges();

            _repository = Kernel.Get<ImageRepositoryInterface>();
            _queryService = Kernel.Get<ImageQueryServiceInterface>();
        }
        
        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        [Test]
        public void TestCreateImageRepository()
        {
            var repository = Kernel.Get<ImageRepositoryInterface>();
            Assert.IsNotNull(repository);
            Assert.IsInstanceOfType<AzureImageRepository>(repository);

            var queryService = Kernel.Get<ImageQueryServiceInterface>();
            Assert.IsNotNull(queryService);
            Assert.IsInstanceOfType<AzureImageRepository>(queryService);
        }

        [Test]
        public ImageInterface TestStoreImageRepository()
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
        public ImageInterface TestQueryImageRepository()
        {
            var img = TestStoreImageRepository();
            return Query(img);
        }

        [Test]
        public void TestQueryModifySaveImageRepository()
        {
            var image = TestQueryImageRepository();
            image.Title = "BlahBlahBlah";
            image.Status = ImageStatus.Ready;
            image.Location = new Location(20,20);
            Store(image);
            Query(image);
        }

        private void Store(ImageInterface source)
        {
            var exists = _queryService.FindById(source.Id) != null;
            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository }))
            {
                if (exists)
                {
                    _repository.UpdateEntity(source.Id, e => e.CopyFieldsFrom(source));
                }
                else
                    _repository.Store(source);
            }
        }

        private ImageInterface Query(ImageInterface source)
        {
            var storedbyid = _queryService.FindById(source.Id);
            var storedbybrowser = _queryService.GetByBrowserId(source.BrowserId).FirstOrDefault();

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
