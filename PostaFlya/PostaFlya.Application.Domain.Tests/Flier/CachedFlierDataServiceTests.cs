﻿using System;
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
using Website.Application.Caching.Command;
using PostaFlya.Application.Domain.Flier.Command;
using PostaFlya.Application.Domain.Flier.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Command;
using PostaFlya.Mocks.Domain.Data;
using Website.Domain.Claims;
using Website.Domain.Comments;
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
            var queryService = kernel.Get<FlierQueryServiceInterface>();

            FlierQueryServiceInterface cachedQueryService = new CachedFlierQueryService(queryService, cache);

            var repository = kernel.Get<FlierRepositoryInterface>();

            var storedFlier = FlierTestData.StoreOne(FlierTestData.GetOne(kernel), repository, kernel);
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
            var queryService = kernel.Get<FlierQueryServiceInterface>();

            FlierQueryServiceInterface cachedQueryService = new CachedFlierQueryService(queryService, cache);

            var cachedFlierRepository = new CachedFlierRepository(kernel.Get<FlierRepositoryInterface>(), cache);

            var storedFlier = FlierTestData.StoreOne(FlierTestData.GetOne(kernel), cachedFlierRepository, kernel);
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
            var queryService = kernel.Get<FlierQueryServiceInterface>();

            FlierQueryServiceInterface cachedQueryService = new CachedFlierQueryService(queryService, cache);

            var cachedFlierRepository = new CachedFlierRepository(kernel.Get<FlierRepositoryInterface>(), cache);

            var storedFlier = FlierTestData.StoreOne(FlierTestData.GetOne(kernel), cachedFlierRepository, kernel);
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
