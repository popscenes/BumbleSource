using System.Linq;
using System.Runtime.Caching;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.TaskJob;
using Website.Application.Caching.Command;
using PostaFlya.Application.Domain.Behaviour.TaskJob.Command;
using PostaFlya.Application.Domain.Behaviour.TaskJob.Query;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using PostaFlya.Mocks.Domain.Data.Behaviour;
using Website.Test.Common;
using TestUtil = Website.Test.Common.TestUtil;

namespace PostaFlya.Application.Domain.Tests.Behaviour.TaskJob
{
    [TestFixture]
    public class CachedTaskJobDataServiceTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsReturnedForTaskJobFindById()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsReturnedForTaskJobFindById(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsReturnedForTaskJobFindById(Kernel, serializeCache);
        }

        public static void CachedDataIsReturnedForTaskJobFindById(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<TaskJobQueryServiceInterface>();

            TaskJobQueryServiceInterface cachedQueryService = new CachedTaskJobQueryService(queryService, cache);

            var repository = kernel.Get<TaskJobRepositoryInterface>();

            var stored = TaskJobTestData.GetOne(kernel);
            TaskJobTestData.StoreOne(stored, repository, kernel);
            var retrieved = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            TaskJobTestData.AssertStoreRetrieve(stored, retrieved);

            const double costOverhead = 2345;
            stored.CostOverhead = costOverhead;
            TaskJobTestData.UpdateOne(stored, repository, kernel);
            retrieved = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);

            Assert.AreNotEqual(retrieved.CostOverhead, costOverhead);

            TestUtil.ClearMemoryCache(cache);

            retrieved = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            TaskJobTestData.AssertStoreRetrieve(stored, retrieved);

        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsReturnedForTaskJobGetBids()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsReturnedForTaskJobGetBids(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsReturnedForTaskJobGetBids(Kernel, serializeCache);
        }

        public static void CachedDataIsReturnedForTaskJobGetBids(MoqMockingKernel kernel, ObjectCache memoryCache)
        {
            var queryService = kernel.Get<TaskJobQueryServiceInterface>();

            TaskJobQueryServiceInterface cachedQueryService = new CachedTaskJobQueryService(queryService, memoryCache);

            var repository = kernel.Get<TaskJobRepositoryInterface>();

            var stored = TaskJobTestData.StoreOne(TaskJobTestData.GetOne(kernel), repository, kernel);
            var retrieved = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            TaskJobTestData.AssertStoreRetrieve(stored, retrieved);

            repository.BidOnTask(TaskJobTestData.GetBid(stored));
            repository.BidOnTask(TaskJobTestData.GetBid(stored));

            var bids = cachedQueryService.GetBids(stored.Id);
            AssertUtil.Count(2, bids);
            repository.BidOnTask(TaskJobTestData.GetBid(stored));
            bids = cachedQueryService.GetBids(stored.Id);
            AssertUtil.Count(2, bids);

            TestUtil.ClearMemoryCache(memoryCache);
            bids = cachedQueryService.GetBids(stored.Id);
            AssertUtil.Count(3, bids);
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsRefreshedWhenUsingCachedRepositoryForTaskJobGetBids()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForTaskJobGetBids(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForTaskJobGetBids(Kernel, serializeCache);
        }

        public static void CachedDataIsRefreshedWhenUsingCachedRepositoryForTaskJobGetBids(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = kernel.Get<TaskJobQueryServiceInterface>();

            TaskJobQueryServiceInterface cachedQueryService = new CachedTaskJobQueryService(queryService, cache);
            var repository = new CachedTaskJobRepository(kernel.Get<TaskJobRepositoryInterface>(), cache, new CacheNotifier());

            var stored = TaskJobTestData.StoreOne(TaskJobTestData.GetOne(kernel), repository, kernel);
            var retrieved = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            TaskJobTestData.AssertStoreRetrieve(stored, retrieved);

            repository.BidOnTask(TaskJobTestData.GetBid(stored));
            repository.BidOnTask(TaskJobTestData.GetBid(stored));

            var bids = cachedQueryService.GetBids(stored.Id);
            Assert.That(bids.Count(), Is.EqualTo(2));
            repository.BidOnTask(TaskJobTestData.GetBid(stored));
            bids = cachedQueryService.GetBids(stored.Id);
            Assert.That(bids.Count(), Is.EqualTo(3));

            TestUtil.ClearMemoryCache(cache);
            bids = cachedQueryService.GetBids(stored.Id);
            Assert.That(bids.Count(), Is.EqualTo(3));
        }


        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsRefreshedWhenUsingCachedRepositoryForTaskJobFindById()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForTaskJobFindById(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForTaskJobFindById(Kernel, serializeCache);
        }

        public static void CachedDataIsRefreshedWhenUsingCachedRepositoryForTaskJobFindById(MoqMockingKernel kernel, ObjectCache memoryCache)
        {
            var queryService = kernel.Get<TaskJobQueryServiceInterface>();


            TaskJobQueryServiceInterface cachedQueryService = new CachedTaskJobQueryService(queryService, memoryCache);

            var cachedRepository = new CachedTaskJobRepository(kernel.Get<TaskJobRepositoryInterface>(), memoryCache, new CacheNotifier());

            var stored = TaskJobTestData.StoreOne(TaskJobTestData.GetOne(kernel), cachedRepository, kernel);
            var retrieved = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            TaskJobTestData.AssertStoreRetrieve(stored, retrieved);

            const double costOverhead = 2345;
            stored.CostOverhead = costOverhead;
            TaskJobTestData.UpdateOne(stored, cachedRepository, kernel);
            retrieved = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);

            TaskJobTestData.AssertStoreRetrieve(stored, retrieved);
        }
    }
}
