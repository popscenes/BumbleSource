using System;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Infrastructure.Caching;
using WebSite.Infrastructure.Caching.Query;
using WebSite.Infrastructure.Command;

namespace WebSite.Application.Caching.Query
{
    public class TimedExpiryCachedQueryService : CachedQueryServiceBase
    {
        private readonly int _defaultSecondsToCache;

        protected TimedExpiryCachedQueryService(ObjectCache cacheProvider
            , string regionName
            , int defaultSecondsToCache = -1) 
            : base(cacheProvider, regionName)
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
