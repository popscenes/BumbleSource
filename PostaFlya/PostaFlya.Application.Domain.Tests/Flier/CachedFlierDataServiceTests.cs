using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Ninject;
using Ninject.MockingKernel.Moq;
using WebSite.Application.Caching.Command;
using PostaFlya.Application.Domain.Flier.Command;
using PostaFlya.Application.Domain.Flier.Query;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Likes;
using WebSite.Infrastructure.Command;
using PostaFlya.Mocks.Domain.Data;
using TestUtil = WebSite.Test.Common.TestUtil;

namespace PostaFlya.Application.Domain.Tests.Flier
{
    [TestFixture]
    public class CachedFlierDataServiceTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsReturnedForFlierFindById()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsReturnedForFlierFindById(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsReturnedForFlierFindById(Kernel, serializeCache);
        }

        public static void CachedDataIsReturnedForFlierFindById(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<FlierQueryServiceInterface>();

            FlierQueryServiceInterface cachedQueryService = new CachedFlierQueryService(queryService, cache);

            var repository = kernel.Get<FlierRepositoryInterface>();

            var storedFlier = FlierTestData.StoreOne(FlierTestData.GetOne(kernel), repository, kernel);
            FlierInterface retrievedFlier = cachedQueryService.FindById(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);

            const string changedDesc = "This will not be in cache";
            storedFlier.Description = changedDesc;
            FlierTestData.UpdateOne(storedFlier, repository, kernel);
            retrievedFlier = cachedQueryService.FindById(storedFlier.Id);

            Assert.AreNotEqual(retrievedFlier.Description, changedDesc);

            TestUtil.ClearMemoryCache(cache);

            retrievedFlier = cachedQueryService.FindById(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);
        }


        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsRefreshedWhenUsingCachedRepositoryForFlierFindById()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForFlierFindById(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForFlierFindById(Kernel, serializeCache);
        }

        public static void CachedDataIsRefreshedWhenUsingCachedRepositoryForFlierFindById(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<FlierQueryServiceInterface>();

            FlierQueryServiceInterface cachedQueryService = new CachedFlierQueryService(queryService, cache);

            var cachedFlierRepository = new CachedFlierRepository(kernel.Get<FlierRepositoryInterface>(), cache, new CacheNotifier());

            var storedFlier = FlierTestData.StoreOne(FlierTestData.GetOne(kernel), cachedFlierRepository, kernel);
            FlierInterface retrievedFlier = cachedQueryService.FindById(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);

            const string changedDesc = "This will be re cached";
            storedFlier.Description = changedDesc;
            FlierTestData.UpdateOne(storedFlier, cachedFlierRepository, kernel);
            retrievedFlier = cachedQueryService.FindById(storedFlier.Id);

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsRefreshedAfterLikeOrComment()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsRefreshedAfterLikeOrComment(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsRefreshedAfterLikeOrComment(Kernel, serializeCache);
        }

        public static void CachedDataIsRefreshedAfterLikeOrComment(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<FlierQueryServiceInterface>();

            FlierQueryServiceInterface cachedQueryService = new CachedFlierQueryService(queryService, cache);

            var cachedFlierRepository = new CachedFlierRepository(kernel.Get<FlierRepositoryInterface>(), cache, new CacheNotifier());

            var storedFlier = FlierTestData.StoreOne(FlierTestData.GetOne(kernel), cachedFlierRepository, kernel);
            FlierInterface retrievedFlier = cachedQueryService.FindById(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);

            storedFlier = cachedFlierRepository.Like(new Like() 
                                                            {   BrowserId = Guid.NewGuid().ToString()
                                                                , EntityId = storedFlier.Id
                                                                , ILike = true
                                                                , LikeTime = DateTime.UtcNow
                                                            }) as FlierInterface;

            retrievedFlier = cachedQueryService.FindById(storedFlier.Id);

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);


            storedFlier = cachedFlierRepository.AddComment(new Comment()
                                                                  {
                                                                      BrowserId = Guid.NewGuid().ToString(),
                                                                      EntityId = storedFlier.Id,
                                                                      CommentTime = DateTime.UtcNow,
                                                                      CommentContent = "123"
                                                                  }) as FlierInterface;

            retrievedFlier = cachedQueryService.FindById(storedFlier.Id);

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);
        }
    }
}
