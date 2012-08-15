using System;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Command;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Browser.Command
{
    internal class CachedBrowserRepository : BroadcastCachedRepository, 
                                             BrowserRepositoryInterface
    {
        private readonly BrowserRepositoryInterface _browserRepository;

        public CachedBrowserRepository([SourceDataSource]BrowserRepositoryInterface browserRepository
            , ObjectCache cacheProvider
            , CacheNotifier notifier)
            : base(cacheProvider, CachedBrowserContext.Region, notifier)
        {
            _browserRepository = browserRepository;
        }

        public void Store(object entity)
        {
            var browser = entity as BrowserInterface;
            if(browser != null) 
                Store(browser);
        }

        public bool SaveChanges()
        {
            return _browserRepository.SaveChanges();
        }

        public void UpdateEntity(string id, Action<BrowserInterface> updateAction)
        {
            Action<BrowserInterface> updateInvCacheAction
            = browser =>
                {
                    //mmm dunno... anyway won't be updating that often, just remove the extenal credentials from the cache in case they are changed by the update operation
                    foreach (var cred in browser.ExternalCredentials)
                        InvalidateCachedData(GetKeyFor(CachedBrowserContext.Identity, cred.GetHash()));

                    InvalidateCachedData(GetKeyFor(CachedBrowserContext.Browser, browser.Id));
                    InvalidateCachedData(GetKeyFor(CachedBrowserContext.Browser, browser.Handle));

                    updateAction(browser);

                };

            _browserRepository.UpdateEntity(id, updateInvCacheAction);
        }

        public void Store(BrowserInterface entity)
        {
            _browserRepository.Store(entity);
        }
    }
}