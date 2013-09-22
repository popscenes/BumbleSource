using System;
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
using Website.Test.Common;
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
            var uow = kernel.Get<UnitOfWorkForRepoInterface>();
            uow.Begin();
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var repository = kernel.Get<GenericRepositoryInterface>();
            var board = BoardTestData.GetAndStoreOne(kernel);

            var storedFlier = FlierTestData.GetOne(kernel);
            storedFlier.AddBoard(board);
            repository.Store(storedFlier);
            repository.SaveChanges();
            FlierInterface retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);

            const string changedDesc = "This will not be in cache";
            storedFlier.Description = changedDesc;
            StoreGetUpdate.UpdateOne(storedFlier, kernel, FlierInterfaceExtensions.CopyFieldsFrom, repository);
            repository.SaveChanges();
            retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);

            Assert.AreNotEqual(retrievedFlier.Description, changedDesc);

            TestUtil.ClearMemoryCache(cache);

            retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);
            uow.End();
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
            var uow = kernel.Get<UnitOfWorkForRepoInterface>();
            uow.Begin();
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var cachedFlierRepository = new CachedRepositoryBase(cache, kernel.Get<GenericRepositoryInterface>());

            var board = BoardTestData.GetAndStoreOne(kernel);

            var storedFlier = FlierTestData.GetOne(kernel);
            storedFlier.AddBoard(board);
            cachedFlierRepository.Store(storedFlier);
            cachedFlierRepository.SaveChanges();
            FlierInterface retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);

            const string changedDesc = "This will be re cached";
            storedFlier.Description = changedDesc;
            StoreGetUpdate.UpdateOne(storedFlier, kernel, FlierInterfaceExtensions.CopyFieldsFrom, cachedFlierRepository);
            cachedFlierRepository.SaveChanges();
            retrievedFlier = cachedQueryService.FindById<PostaFlya.Domain.Flier.Flier>(storedFlier.Id);

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFlier);
            uow.End();
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

            var board = BoardTestData.GetAndStoreOne(kernel);

            var storedFlier = FlierTestData.GetOne(kernel);
            storedFlier.AddBoard(board);
            cachedFlierRepository.Store(storedFlier);
            cachedFlierRepository.SaveChanges();
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
