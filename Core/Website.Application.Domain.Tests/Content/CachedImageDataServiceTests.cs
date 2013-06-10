using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Caching.Query;
using Website.Application.Domain.Browser.Query;
using Website.Infrastructure.Caching.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Test.Common;
using Website.Domain.Content;
using Website.Mocks.Domain.Data;

namespace Website.Application.Domain.Tests.Content
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
            var queryService = ResolutionExtensions.Get<GenericQueryServiceInterface>(kernel);
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var repository = ResolutionExtensions.Get<GenericRepositoryInterface>(kernel);

            var storedImage = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel), repository, kernel);
            var retrievedImage = cachedQueryService.FindById<Image>(storedImage.Id);
            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);

            const string changedName = "This will not be in cache";
            storedImage.Title = changedName;
            DomainImageTestData.UpdateOne(storedImage, repository, kernel);
            retrievedImage = cachedQueryService.FindById<Image>(storedImage.Id);

            Assert.AreNotEqual(retrievedImage.Title, changedName);

            TestUtil.ClearMemoryCache(cache);

            retrievedImage = cachedQueryService.FindById<Image>(storedImage.Id);
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
            var queryService = ResolutionExtensions.Get<GenericQueryServiceInterface>(kernel);
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var cachedImageRepository = new CachedRepositoryBase(cache, kernel.Get<GenericRepositoryInterface>());

            var storedImage = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel), cachedImageRepository, kernel);
            ImageInterface retrievedImage = cachedQueryService.FindById<Image>(storedImage.Id);
            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);

            const string changedName = "This won't be cached";
            storedImage.Title = changedName;
            DomainImageTestData.UpdateOne(storedImage, cachedImageRepository, kernel);
            retrievedImage = cachedQueryService.FindById<Image>(storedImage.Id);

            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);

        }

//        [Test]//note implement the same in other application test projects for different cache implementations
//        public void ImageInfoIsNotCachedWhenStatusIsProcessingForImageFindById()
//        {           
//            var memoryCache = TestUtil.GetMemoryCache();
//            ImageInfoIsNotCachedWhenStatusIsProcessingForImageFindById(Kernel, memoryCache);
//            memoryCache.Dispose();
//
//            var serializeCache = TestUtil.GetSerializingCache();
//            ImageInfoIsNotCachedWhenStatusIsProcessingForImageFindById(Kernel, serializeCache);
//        }
//
//        public static void ImageInfoIsNotCachedWhenStatusIsProcessingForImageFindById(MoqMockingKernel kernel, ObjectCache cache)
//        {
//            var queryService = kernel.Get<ImageQueryServiceInterface>();
//            ImageQueryServiceInterface cachedQueryService = new CachedImageQueryService(queryService, cache);
//
//            //just use normal repo not one that deletes cached entries
//            var imageRepository = kernel.Get<ImageRepositoryInterface>();
//
//            var img = DomainImageTestData.GetOne(kernel);
//            img.Status = ImageStatus.Processing;
//            var storedImage = DomainImageTestData.StoreOne(img, imageRepository, kernel);
//            ImageInterface retrievedImage = cachedQueryService.FindById<Image>(storedImage.Id);
//            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);
//
//            string changedName = "This wont be cached";
//            storedImage.Title = changedName;
//            DomainImageTestData.UpdateOne(storedImage, imageRepository, kernel);
//            retrievedImage = cachedQueryService.FindById<Image>(storedImage.Id);
//            DomainImageTestData.AssertStoreRetrieve(storedImage, retrievedImage);
//
//            changedName = "This will be cached";
//            storedImage.Title = changedName;
//            storedImage.Status = ImageStatus.Ready;
//            DomainImageTestData.UpdateOne(storedImage, imageRepository, kernel);
//            retrievedImage = cachedQueryService.FindById<Image>(storedImage.Id);
//
//            changedName = "This wont be updated";
//            storedImage.Title = changedName;
//            DomainImageTestData.UpdateOne(storedImage, imageRepository, kernel);
//            retrievedImage = cachedQueryService.FindById<Image>(storedImage.Id);
//            Assert.AreNotEqual(retrievedImage.Title, changedName);
//        }

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
            var queryService = ResolutionExtensions.Get<GenericQueryServiceInterface>(kernel);
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var repository = kernel.Get<GenericRepositoryInterface>();

            var browserId = Guid.NewGuid().ToString();
            var img = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), repository, kernel);
            DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), repository, kernel);

            var queryChannel = kernel.Get<QueryChannelInterface>();
            var retrievedImages = queryChannel.Query(new GetByBrowserIdQuery() {BrowserId = browserId}, (List<Image>) null);

            Assert.That(retrievedImages.Count(), Is.EqualTo(2));
            DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), repository, kernel);

            retrievedImages = queryChannel.Query(new GetByBrowserIdQuery() { BrowserId = browserId }, (List<Image>)null);
            Assert.That(retrievedImages.Count(), Is.EqualTo(2));

            var cacheStore = (cache.ToList().FirstOrDefault(kv => 
                kv.Key.Contains("BrowserId")).Value as IEnumerable<string>);
            Assert.That(cacheStore.Count(), Is.EqualTo(2));

            TestUtil.ClearMemoryCache(cache);

            retrievedImages = queryChannel.Query(new GetByBrowserIdQuery() { BrowserId = browserId }, (List<Image>)null);
            Assert.That(retrievedImages.Count(), Is.EqualTo(3));
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
            var queryService = ResolutionExtensions.Get<GenericQueryServiceInterface>(kernel);
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var cachedImageRepository = new CachedRepositoryBase(cache, kernel.Get<GenericRepositoryInterface>());
            var browserId = Guid.NewGuid().ToString();
            var storedImage = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), cachedImageRepository, kernel);

            var queryChannel = kernel.Get<QueryChannelInterface>();
            var retrievedImages = queryChannel.Query(new GetByBrowserIdQuery() { BrowserId = storedImage.BrowserId }, (List<Image>)null);

            Assert.That(retrievedImages.Count(), Is.EqualTo(1));

            storedImage = DomainImageTestData.StoreOne(DomainImageTestData.GetOne(kernel, browserId), cachedImageRepository, kernel);
            retrievedImages = queryChannel.Query(new GetByBrowserIdQuery() { BrowserId = storedImage.BrowserId }, (List<Image>)null);

            Assert.That(retrievedImages.Count(), Is.EqualTo(2));
        }
    }
}
