using System;
using System.Runtime.Caching;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Caching.Command;
using Website.Application.Caching.Query;
using Website.Infrastructure.Command;
using Website.Test.Common;

namespace Website.Application.Tests.Caching
{
    public class CachedRepositoryTest : BroadcastCachedRepository
    {
        private readonly ObjectCache _cacheProvider;

        public CachedRepositoryTest(ObjectCache cacheProvider
            , string regionName
            , CacheNotifier notifier)
            : base(cacheProvider, regionName, notifier, null)
        {
            _cacheProvider = cacheProvider;
        }

        public CacheItem GetCacheItem(string key)
        {
            return _cacheProvider.GetCacheItem(GetInternalKey(key), GetRegion());
        }

        public void TestRemoveKey(string key)
        {
            this.InvalidateCachedData(key);
        }
    }

    public class CachedQueryServiceTest : TimedExpiryCachedQueryService
    {
        private readonly ObjectCache _cacheProvider;

        public CachedQueryServiceTest(ObjectCache cacheProvider
            , string regionName
            , int defaultSecondsToCache)
            : base(cacheProvider, regionName, null, defaultSecondsToCache)
        {
            _cacheProvider = cacheProvider;
        }

        public string GetCachedData(string key)
        {
            return RetrieveCachedData(key, () => "cacheddata");
        }

        public CacheItem GetCacheItem(string key)
        {
            return _cacheProvider.GetCacheItem(GetInternalKey(key), GetRegion());
        }

        public CacheItemPolicy GetPolicy()
        {
            return GetDefaultPolicy();
        }
    }

    [TestFixture]
    public class CachedDataSourceBaseTests
    {
        private MemoryCache _memoryCache;

        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _memoryCache = TestUtil.GetMemoryCache();
            Kernel.Bind<ObjectCache>().ToConstant(_memoryCache);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<ObjectCache>();
        }

        [Test]
        public void CachedDataSourceBaseInvalidateCachedDataBroadcastsInvalidateCacheDataCommand()
        {
            var localCache = TestUtil.GetMemoryCache();
            var queryServ = new CachedQueryServiceTest(localCache, "testreg", 60);
            queryServ.GetCachedData("testkey");
            var cacheEntry = queryServ.GetCacheItem("testkey");

            var copyitem = new CacheItem(cacheEntry.Key, cacheEntry.Value, cacheEntry.RegionName);
            _memoryCache.Add(copyitem, queryServ.GetPolicy());

            AssertUtil.AssertAreElementsEqualForKeyValPairsIncludeEnumerableValues(localCache, _memoryCache);
            _memoryCache.Add("anotherkey", "anotherval", DateTime.UtcNow.AddMinutes(100));

            var notifier = new CacheNotifier(Kernel.Get<DefaultCommandBus>());
            var repo = new CachedRepositoryTest(localCache
                , "testreg"
                , notifier);
            repo.TestRemoveKey("testkey");

            Assert.IsFalse(_memoryCache.Contains(copyitem.Key, copyitem.RegionName));
            Assert.IsTrue(_memoryCache.Contains("anotherkey"));//still contains the other key
        }

    }
}
