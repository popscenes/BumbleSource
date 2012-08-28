using System;
using System.Linq;
using System.Runtime.Caching;
using Website.Application.Caching.Query;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;

namespace Website.Application.Domain.Query
{
    public class CachedQueryServiceWithBrowser 
        : TimedExpiryCachedQueryService, QueryByBrowserInterface
    {
        private readonly QueryByBrowserInterface _queryService;
        public CachedQueryServiceWithBrowser(ObjectCache cacheProvider
            , string regionName
            , QueryServiceWithBrowserInterface queryService
            , int defaultSecondsToCache)
            : base(cacheProvider, regionName, queryService, defaultSecondsToCache)
        {
            _queryService = queryService;
        }

        public IQueryable<EntityType> GetByBrowserId<EntityType>(String browserId) where EntityType : class, BrowserIdInterface, new()
        {
            return RetrieveCachedData(
                GetKeyFor("forbrowser", browserId),
                () => this._queryService.GetByBrowserId<EntityType>(browserId).ToList())
                .AsQueryable();
        }
    }
}
