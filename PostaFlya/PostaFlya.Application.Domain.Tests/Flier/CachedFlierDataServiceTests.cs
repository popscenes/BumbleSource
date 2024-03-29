﻿using System;
using System.Runtime.Caching;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Flier;
using PostaFlya.Mocks.Domain.Data;
using Website.Application.Caching.Query;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Infrastructure.Caching.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using TestUtil = Website.Test.Common.TestUtil;

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
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var repository = kernel.Get<GenericRepositoryInterface>();
            var board = BoardTestData.GetAndStoreOne(kernel, repository);

            var storedFlier = FlierTestData.StoreOne(FlierTestData.GetOne(kernel), repository, kernel, board);
            FlierInterface retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);

            const string changedDesc = "This will not be in cache";
            storedFlier.Description = changedDesc;
            FlierTestData.UpdateOne(storedFlier, repository, kernel);
            retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);

            Assert.AreNotEqual(retrievedFlier.Description, changedDesc);

            TestUtil.ClearMemoryCache(cache);

            retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);
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
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var cachedFlierRepository = new CachedRepositoryBase(cache, kernel.Get<GenericRepositoryInterface>());

            var board = BoardTestData.GetAndStoreOne(kernel, cachedFlierRepository);

            var storedFlier = FlierTestData.StoreOne(FlierTestData.GetOne(kernel), cachedFlierRepository, kernel, board);
            FlierInterface retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);

            const string changedDesc = "This will be re cached";
            storedFlier.Description = changedDesc;
            FlierTestData.UpdateOne(storedFlier, cachedFlierRepository, kernel);
            retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsRefreshedAfterClaimOrComment()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsRefreshedAfterClaimOrComment(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsRefreshedAfterClaimOrComment(Kernel, serializeCache);
        }

        public static void CachedDataIsRefreshedAfterClaimOrComment(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var cachedFlierRepository = new CachedRepositoryBase(cache, kernel.Get<GenericRepositoryInterface>());

            var board = BoardTestData.GetAndStoreOne(kernel, cachedFlierRepository);

            var storedFlier = FlierTestData.StoreOne(FlierTestData.GetOne(kernel), cachedFlierRepository, kernel, board);
            FlierInterface retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);

            cachedFlierRepository.Store(new Claim() 
                    {   BrowserId = Guid.NewGuid().ToString()
                        , AggregateId = storedFlier.Id
                        , ClaimTime = DateTime.UtcNow
                    });

            retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);


            cachedFlierRepository.Store(new Comment()
                    {
                        BrowserId = Guid.NewGuid().ToString(),
                        AggregateId = storedFlier.Id,
                        CommentTime = DateTime.UtcNow,
                        CommentContent = "123"
                    });

            retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);
        }
    }
}
