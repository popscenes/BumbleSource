using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Query;
using WebSite.Application.Caching.Query;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Application.Domain.Query
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
