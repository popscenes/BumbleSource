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
using WebSite.Application.Binding;
using WebSite.Application.Caching.Command;
using PostaFlya.Application.Domain.Browser.Command;
using PostaFlya.Application.Domain.Browser.Query;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.TaskJob.Query;
using WebSite.Infrastructure.Caching;
using PostaFlya.Mocks.Domain.Data;
using TestUtil = WebSite.Test.Common.TestUtil;

namespace PostaFlya.Application.Domain.Tests.Browser
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
            var queryService = kernel.Get<BrowserQueryServiceInterface>();
            BrowserQueryServiceInterface cachedQueryService = new CachedBrowserQueryService(queryService, cache);
            var repository = kernel.Get<BrowserRepositoryInterface>();

            var storedBrowser = BrowserTestData.StoreOne(BrowserTestData.GetOne(kernel), repository, kernel);
            var retrievedBrowser = cachedQueryService.FindById(storedBrowser.Id);
            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);

            const string changedName = "This will not be in cache";
            storedBrowser.Handle = changedName;
            BrowserTestData.UpdateOne(storedBrowser, repository, kernel);
            retrievedBrowser = cachedQueryService.FindById(storedBrowser.Id);

            Assert.AreNotEqual(retrievedBrowser.Handle, changedName);

            TestUtil.ClearMemoryCache(cache);

            retrievedBrowser = cachedQueryService.FindById(storedBrowser.Id);
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
            var queryService = kernel.Get<BrowserQueryServiceInterface>();

            BrowserQueryServiceInterface cachedQueryService = new CachedBrowserQueryService(queryService, cache);

            var cachedBrowserRepository = new CachedBrowserRepository(kernel.Get<BrowserRepositoryInterface>(), cache, new CacheNotifier());

            var storedBrowser = BrowserTestData.StoreOne(BrowserTestData.GetOne(kernel), cachedBrowserRepository, kernel);
            BrowserInterface retrievedBrowser = cachedQueryService.FindById(storedBrowser.Id);
            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);

            const string changedName = "This will be re cached";
            storedBrowser.Handle = changedName;
            BrowserTestData.UpdateOne(storedBrowser, cachedBrowserRepository, kernel);
            retrievedBrowser = cachedQueryService.FindById(storedBrowser.Id);

            BrowserTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);
        }
    }
}
