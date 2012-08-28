using System.Runtime.Caching;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Caching.Command;
using Website.Test.Common;
using Website.Application.Domain.Browser.Command;
using Website.Application.Domain.Browser.Query;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Query;
using Website.Mocks.Domain.Data;

namespace Website.Application.Domain.Tests.Browser
{
    [TestFixture]
    public class CachedBrowserDataServiceTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsReturnedForBrowserFindById()
        {       
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsReturnedForBrowserFindById(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsReturnedForBrowserFindById(Kernel, serializeCache);
        }

        public static void CachedDataIsReturnedForBrowserFindById(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = ResolutionExtensions.Get<BrowserQueryServiceInterface>(kernel);
            BrowserQueryServiceInterface cachedQueryService = new CachedBrowserQueryService(queryService, cache);
            var repository = ResolutionExtensions.Get<BrowserRepositoryInterface>(kernel);

            var storedBrowser = BrowserTestData.StoreOne(BrowserTestData.GetOne(kernel), repository, kernel);
            var retrievedBrowser = cachedQueryService.FindById<Website.Domain.Browser.Browser>(storedBrowser.Id);
            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);

            const string changedName = "This will not be in cache";
            storedBrowser.Handle = changedName;
            BrowserTestData.UpdateOne(storedBrowser, repository, kernel);
            retrievedBrowser = cachedQueryService.FindById<Website.Domain.Browser.Browser>(storedBrowser.Id);

            Assert.AreNotEqual(retrievedBrowser.Handle, changedName);

            TestUtil.ClearMemoryCache(cache);

            retrievedBrowser = cachedQueryService.FindById<Website.Domain.Browser.Browser>(storedBrowser.Id);
            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);
        }


        [Test]//note implement the same in other application test projects for different cache implementations
        public void CachedDataIsRefreshedWhenUsingCachedRepositoryForBrowserFindById()
        {
            var memoryCache = TestUtil.GetMemoryCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForBrowserFindById(Kernel, memoryCache);
            memoryCache.Dispose();

            var serializeCache = TestUtil.GetSerializingCache();
            CachedDataIsRefreshedWhenUsingCachedRepositoryForBrowserFindById(Kernel, serializeCache);
        }

        public static void CachedDataIsRefreshedWhenUsingCachedRepositoryForBrowserFindById(MoqMockingKernel kernel, ObjectCache cache)
        {
            var queryService = ResolutionExtensions.Get<BrowserQueryServiceInterface>(kernel);

            BrowserQueryServiceInterface cachedQueryService = new CachedBrowserQueryService(queryService, cache);

            var cachedBrowserRepository = new CachedBrowserRepository(ResolutionExtensions.Get<BrowserRepositoryInterface>(kernel), cache, new CacheNotifier());

            var storedBrowser = BrowserTestData.StoreOne(BrowserTestData.GetOne(kernel), cachedBrowserRepository, kernel);
            BrowserInterface retrievedBrowser = cachedQueryService.FindById<Website.Domain.Browser.Browser>(storedBrowser.Id);
            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);

            const string changedName = "This will be re cached";
            storedBrowser.Handle = changedName;
            BrowserTestData.UpdateOne(storedBrowser, cachedBrowserRepository, kernel);
            retrievedBrowser = cachedQueryService.FindById<Website.Domain.Browser.Browser>(storedBrowser.Id);

            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);
        }
    }
}
