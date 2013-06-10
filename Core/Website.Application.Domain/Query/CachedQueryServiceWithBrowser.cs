//using System;
//using System.Linq;
//using System.Runtime.Caching;
//using Website.Application.Caching.Query;
//using Website.Domain.Browser;
//using Website.Domain.Browser.Query;
//using Website.Infrastructure.Binding;
//using Website.Infrastructure.Caching.Query;
//
//namespace Website.Application.Domain.Query
//{
//    public class CachedQueryServiceWithBrowser 
//        : TimedExpiryCachedQueryService, GenericQueryServiceInterface
//    {
//        private readonly GenericQueryServiceInterface _queryService;
//        public CachedQueryServiceWithBrowser(ObjectCache cacheProvider
//            , [SourceDataSource] GenericQueryServiceInterface queryService
//            , int defaultSecondsToCache = -1)
//            : base(cacheProvider, queryService, defaultSecondsToCache)
//        {
//            _queryService = queryService;
//        }
//
//        public IQueryable<string> GetEntityIdsByBrowserId<EntityType>(String browserId) where EntityType : class, BrowserIdInterface, new()
//        {
//            return RetrieveCachedData(browserId.GetCacheKeyFor<EntityType>("BrowserId"),
//                () => this._queryService.GetEntityIdsByBrowserId<EntityType>(browserId).ToList())
//                .AsQueryable();
//        }
//    }
//}
