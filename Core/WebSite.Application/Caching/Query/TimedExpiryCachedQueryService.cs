using System;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Infrastructure.Binding;
using WebSite.Infrastructure.Caching;
using WebSite.Infrastructure.Caching.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;

namespace WebSite.Application.Caching.Query
{
    public class TimedExpiryCachedQueryService : CachedQueryServiceBase
    {
        private readonly int _defaultSecondsToCache;

        public TimedExpiryCachedQueryService(ObjectCache cacheProvider
            , string regionName
            , [SourceDataSource]GenericQueryServiceInterface genericQueryService
            , int defaultSecondsToCache = -1)
            : base(cacheProvider, regionName, genericQueryService)
        {
            _defaultSecondsToCache = defaultSecondsToCache;
        }

        protected override CacheItemPolicy GetDefaultPolicy()
        {
            return new CacheItemPolicy
                       {                
                           AbsoluteExpiration = _defaultSecondsToCache < 0 ?                                                                            DateTimeOffset.MaxValue
                           :DateTime.UtcNow.AddSeconds(_defaultSecondsToCache)
                       };
        }

    }
}
