using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Ninject;
using Ninject.MockingKernel.Moq;
using WebSite.Application.Caching.Command;
using PostaFlya.Application.Domain.Content.Command;
using PostaFlya.Application.Domain.Content.Query;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using PostaFlya.Domain.Content.Query;
using PostaFlya.Mocks.Domain.Data;
using TestUtil = WebSite.Test.Common.TestUtil;

namespace PostaFlya.Application.Domain.Tests.Content
{
    [TestFixture]
    public class CachedImageDataServiceTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsReturnedForImageFindById()
        {        
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsReturnedForImageFindById(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsReturnedForImageFindById(Kernel, serializeCache);
        }

        public static void CachedDataIsReturnedForImageFindById(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<ImageQueryServiceInterface>();
            ImageQueryServiceInterface cachedQueryService = new CachedImageQueryService(queryService, cache);

            var repository = kernel.Get<ImageRepositoryInterface>();

            var storedImage = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel), repository, kernel);
            var retrievedImage = cachedQueryService.FindById(storedImage.Id);
            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);

            const string changedName = "This will not be in cache";
            storedImage.Title = changedName;
            DomainImageTestData.UpdateOne(storedImage, repository, kernel);
            retrievedImage = cachedQueryService.FindById(storedImage.Id);

            Assert.AreNotEqual(retrievedImage.Title, changedName);

            TestUtil.ClearMemoryCache(cache);

            retrievedImage = cachedQueryService.FindById(storedImage.Id);
            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);
        }


        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsRefreshedWhenUsingCachedRepositoryForImageFindById()
        {       
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForImageFindById(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForImageFindById(Kernel, serializeCache);
        }

        public static void CachedDataIsRefreshedWhenUsingCachedRepositoryForImageFindById(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<ImageQueryServiceInterface>();
            ImageQueryServiceInterface cachedQueryService = new CachedImageQueryService(queryService, cache);

            var cachedImageRepository = new CachedImageRepository(kernel.Get<ImageRepositoryInterface>(), cache, new CacheNotifier());

            var storedImage = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel), cachedImageRepository, kernel);
            ImageInterface retrievedImage = cachedQueryService.FindById(storedImage.Id);
            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);

            const string changedName = "This won't be cached";
            storedImage.Title = changedName;
            DomainImageTestData.UpdateOne(storedImage, cachedImageRepository, kernel);
            retrievedImage = cachedQueryService.FindById(storedImage.Id);

            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);

        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void ImageInfoIsNotCachedWhenStatusIsProcessingForImageFindById()
        {           
            var memoryCache = TestUtil.GetMemoryCache();
            ImageInfoIsNotCachedWhenStatusIsProcessingForImageFindById(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            ImageInfoIsNotCachedWhenStatusIsProcessingForImageFindById(Kernel, serializeCache);
        }

        public static void ImageInfoIsNotCachedWhenStatusIsProcessingForImageFindById(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<ImageQueryServiceInterface>();
            ImageQueryServiceInterface cachedQueryService = new CachedImageQueryService(queryService, cache);

            //just use normal repo not one that deletes cached entries
            var imageRepository = kernel.Get<ImageRepositoryInterface>();

            var img = DomainImageTestData.GetOne(kernel);
            img.Status = ImageStatus.Processing;
            var storedImage = DomainImageTestData.StoreOne(img, imageRepository, kernel);
            ImageInterface retrievedImage = cachedQueryService.FindById(storedImage.Id);
            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);

            string changedName = "This wont be cached";
            storedImage.Title = changedName;
            DomainImageTestData.UpdateOne(storedImage, imageRepository, kernel);
            retrievedImage = cachedQueryService.FindById(storedImage.Id);
            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);

            changedName = "This will be cached";
            storedImage.Title = changedName;
            storedImage.Status = ImageStatus.Ready;
            DomainImageTestData.UpdateOne(storedImage, imageRepository, kernel);
            retrievedImage = cachedQueryService.FindById(storedImage.Id);

            changedName = "This wont be updated";
            storedImage.Title = changedName;
            DomainImageTestData.UpdateOne(storedImage, imageRepository, kernel);
            retrievedImage = cachedQueryService.FindById(storedImage.Id);
            Assert.AreNotEqual(retrievedImage.Title, changedName);
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsReturnedForImageGetByBrowserId()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsReturnedForImageGetByBrowserId(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsReturnedForImageGetByBrowserId(Kernel, serializeCache);
        }

        public static void CachedDataIsReturnedForImageGetByBrowserId(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<ImageQueryServiceInterface>();

            ImageQueryServiceInterface cachedQueryService = new CachedImageQueryService(queryService, cache);

            var repository = kernel.Get<ImageRepositoryInterface>();

            var browserId = Guid.NewGuid().ToString();
            var img = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), repository, kernel);
            DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), repository, kernel);

            var retrievedImages = cachedQueryService.GetByBrowserId(browserId);
            Assert.Count(2, retrievedImages);
            DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), repository, kernel);
            retrievedImages = cachedQueryService.GetByBrowserId(browserId);
            Assert.Count(2, retrievedImages);
            Assert.Count(2, cache.ToList().FirstOrDefault().Value as IEnumerable);

            TestUtil.ClearMemoryCache(cache);

            retrievedImages = cachedQueryService.GetByBrowserId(img.BrowserId);
            Assert.Count(3, retrievedImages);
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsInvalidatedByStoreForImageGetByBrowserId()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsInvalidatedByStoreForImageGetByBrowserId(Kernel, memoryCache);
            memoryCache.Dispose();


            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsInvalidatedByStoreForImageGetByBrowserId(Kernel, serializeCache);
        }

        public static void CachedDataIsInvalidatedByStoreForImageGetByBrowserId(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<ImageQueryServiceInterface>();

            ImageQueryServiceInterface cachedQueryService = new CachedImageQueryService(queryService, cache);

            var cachedImageRepository = new CachedImageRepository(kernel.Get<ImageRepositoryInterface>(), cache, new CacheNotifier());
            var browserId = Guid.NewGuid().ToString();
            var storedImage = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), cachedImageRepository, kernel);
            var retrievedImages = cachedQueryService.GetByBrowserId(storedImage.BrowserId);


            Assert.Count(1, retrievedImages);

            storedImage = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), cachedImageRepository, kernel);
            retrievedImages = cachedQueryService.GetByBrowserId(storedImage.BrowserId);

            Assert.Count(2, retrievedImages);
        }
    }
}
