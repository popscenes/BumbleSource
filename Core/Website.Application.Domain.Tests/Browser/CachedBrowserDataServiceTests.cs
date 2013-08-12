using System.Runtime.Caching;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Caching.Query;
using Website.Infrastructure.Caching.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Test.Common;
using Website.Domain.Browser;
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
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);
            var repository = kernel.Get<GenericRepositoryInterface>();

            var storedBrowser = BrowserTestData.StoreOne(BrowserTestData.GetOne(kernel), repository, kernel);
            var retrievedBrowser = cachedQueryService.FindById<Website.Domain.Browser.Browser>(storedBrowser.Id);
            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);

            const string changedName = "This will not be in cache";
            storedBrowser.FriendlyId = changedName;
            BrowserTestData.UpdateOne(storedBrowser, repository, kernel);
            retrievedBrowser = cachedQueryService.FindById<Website.Domain.Browser.Browser>(storedBrowser.Id);

            Assert.AreNotEqual(retrievedBrowser.FriendlyId, changedName);

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
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            GenericQueryServiceInterface cachedQueryService = new TimedExpiryCachedQueryService(cache, queryService);

            var cachedBrowserRepository = new CachedRepositoryBase(cache, kernel.Get<GenericRepositoryInterface>());

            var storedBrowser = BrowserTestData.StoreOne(BrowserTestData.GetOne(kernel), cachedBrowserRepository, kernel);
            BrowserInterface retrievedBrowser = cachedQueryService.FindById<Website.Domain.Browser.Browser>(storedBrowser.Id);
            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);

            const string changedName = "This will be re cached";
            storedBrowser.FriendlyId = changedName;
            BrowserTestData.UpdateOne(storedBrowser, cachedBrowserRepository, kernel);
            retrievedBrowser = cachedQueryService.FindById<Website.Domain.Browser.Browser>(storedBrowser.Id);

            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);
        }
    }
}
