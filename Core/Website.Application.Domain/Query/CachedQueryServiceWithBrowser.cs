using System;
using System.Linq;
using System.Runtime.Caching;
using Website.Application.Caching.Query;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;

namespace Website.Application.Domain.Query
{
    public class CachedQueryServiceWithBrowser 
        : TimedExpiryCachedQueryService, QueryServiceForBrowserAggregateInterface
    {
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        public CachedQueryServiceWithBrowser(ObjectCache cacheProvider
            , string regionName
            , QueryServiceForBrowserAggregateInterface queryService
            , int defaultSecondsToCache)
            : base(cacheProvider, regionName, queryService, defaultSecondsToCache)
        {
            _queryService = queryService;
        }

        public IQueryable<string> GetEntityIdsByBrowserId<EntityType>(String browserId) where EntityType : class, BrowserIdInterface, new()
        {
            return RetrieveCachedData(
                GetKeyFor(BrowserIdPrefix(typeof(EntityType)), browserId),
                () => this._queryService.GetEntityIdsByBrowserId<EntityType>(browserId).ToList())
                .AsQueryable();
        }

        public static string BrowserIdPrefix(Type entityTyp)
        {
            return entityTyp.FullName + " by browser";
        }
    }
}
