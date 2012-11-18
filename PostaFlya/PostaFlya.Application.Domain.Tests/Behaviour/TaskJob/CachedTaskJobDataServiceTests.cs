using System.Runtime.Caching;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.TaskJob;
using Website.Application.Caching.Command;
using PostaFlya.Mocks.Domain.Data.Behaviour;
using Website.Application.Caching.Query;
using Website.Infrastructure.Caching.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
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
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var repository = kernel.Get<GenericRepositoryInterface>();

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
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(memoryCache, queryService);

            var repository = kernel.Get<GenericRepositoryInterface>();

            var stored = TaskJobTestData.StoreOne(TaskJobTestData.GetOne(kernel), repository, kernel);

            repository.UpdateEntity<TaskJobFlierBehaviour>(stored.Id, task =>
                {
                    task.Bids.Add(TaskJobTestData.GetBid(stored));
                    task.Bids.Add(TaskJobTestData.GetBid(stored));
                });
            
            var taskJob = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            AssertUtil.Count(2, taskJob.Bids);

            repository.UpdateEntity<TaskJobFlierBehaviour>(stored.Id, task => task.Bids.Add(TaskJobTestData.GetBid(stored)));

            taskJob = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            AssertUtil.Count(2, taskJob.Bids);

            TestUtil.ClearMemoryCache(memoryCache);
            taskJob = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            AssertUtil.Count(3, taskJob.Bids);
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
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var repository = new CachedRepositoryBase(cache, kernel.Get<GenericRepositoryInterface>());

            var stored = TaskJobTestData.StoreOne(TaskJobTestData.GetOne(kernel), repository, kernel);
            var retrieved = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            TaskJobTestData.AssertStoreRetrieve(stored, retrieved);

            repository.UpdateEntity<TaskJobFlierBehaviour>(stored.Id, task =>
                {
                    task.Bids.Add(TaskJobTestData.GetBid(stored));
                    task.Bids.Add(TaskJobTestData.GetBid(stored));
                });

            var taskJob = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            Assert.That(taskJob.Bids.Count, Is.EqualTo(2));

            repository.UpdateEntity<TaskJobFlierBehaviour>(stored.Id, task => task.Bids.Add(TaskJobTestData.GetBid(stored)));
            taskJob = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            Assert.That(taskJob.Bids.Count, Is.EqualTo(3));

            TestUtil.ClearMemoryCache(cache);
            taskJob = cachedQueryService.FindById<TaskJobFlierBehaviour>(stored.Id);
            Assert.That(taskJob.Bids.Count, Is.EqualTo(3));
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
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(memoryCache, queryService);

            var cachedRepository = new CachedRepositoryBase(memoryCache, kernel.Get<GenericRepositoryInterface>());

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
