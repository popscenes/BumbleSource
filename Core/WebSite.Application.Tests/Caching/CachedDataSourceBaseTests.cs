using System.Runtime.Caching;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Caching.Query;
using Website.Test.Common;

namespace Website.Application.Tests.Caching
{

    public class CachedQueryServiceTest : TimedExpiryCachedQueryService
    {
        private readonly ObjectCache _cacheProvider;

        public CachedQueryServiceTest(ObjectCache cacheProvider
            , int defaultSecondsToCache)
            : base(cacheProvider, null, defaultSecondsToCache)
        {
            _cacheProvider = cacheProvider;
        }

        public string GetCachedData(string key)
        {
            return RetrieveCachedData(key, () => "cacheddata");
        }

        public CacheItem GetCacheItem(string key)
        {
            return _cacheProvider.GetCacheItem(key);
        }

        public CacheItemPolicy GetPolicy()
        {
            return GetDefaultPolicy();
        }
    }

//    [TestFixture]
//    public class CachedDataSourceBaseTests
//    {
//        private MemoryCache _memoryCache;
//
//        MoqMockingKernel Kernel
//        {
//            get { return TestFixtureSetup.CurrIocKernel; }
//        }
//
//        [TestFixtureSetUp]
//        public void FixtureSetUp()
//        {
//            _memoryCache = TestUtil.GetMemoryCache();
//            Kernel.Bind<ObjectCache>().ToConstant(_memoryCache);
//        }
//
//        [TestFixtureTearDown]
//        public void FixtureTearDown()
//        {
//            Kernel.Unbind<ObjectCache>();
//        }
//
//
//    }
}
