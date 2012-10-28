using System;
using System.Runtime.Caching;
using Website.Application.Caching.Command;
using Website.Infrastructure.Binding;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Infrastructure.Domain;

namespace Website.Application.Domain.Browser.Command
{
    internal class CachedBrowserRepository : BroadcastCachedRepository, 
                                             BrowserRepositoryInterface
    {
        public CachedBrowserRepository([SourceDataSource]BrowserRepositoryInterface browserRepository
            , ObjectCache cacheProvider
            , CacheNotifier notifier)
            : base(cacheProvider, CachedBrowserContext.Region, notifier, browserRepository)
        {
        }

        public override void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) 
        {
            Action<UpdateType> updateInvCacheAction
            = browser =>
                  {
                    var target = browser as BrowserInterface;
                    //mmm dunno... anyway won't be updating that often, just remove the extenal credentials from the cache in case they are changed by the update operation
                    foreach (var cred in target.ExternalCredentials)
                        InvalidateCachedData(GetKeyFor(CachedBrowserContext.Identity, cred.GetHash()));

                    updateAction(browser);
                };

            base.UpdateEntity(id, updateInvCacheAction);
        }
    }
}