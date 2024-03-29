using System;
using System.Runtime.Caching;
using Website.Application.Binding;
using Website.Infrastructure.Caching;
using Website.Infrastructure.Command;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Caching.Query;
using Website.Infrastructure.Query;

namespace Website.Application.Caching.Query
{
    public class TimedExpiryCachedQueryService : CachedQueryServiceBase
    {
        private readonly int _defaultSecondsToCache;

        public TimedExpiryCachedQueryService(ObjectCache cacheProvider
            , [SourceDataSource] GenericQueryServiceInterface genericQueryService
            , int defaultSecondsToCache = 1800)
            : base(cacheProvider, genericQueryService)
        {
            _defaultSecondsToCache = defaultSecondsToCache;
        }

        protected override CacheItemPolicy GetDefaultPolicy()
        {
            return new CacheItemPolicy
                       {                
                           AbsoluteExpiration = _defaultSecondsToCache < 0 ? 
                           DateTimeOffset.MaxValue :
                           DateTimeOffset.UtcNow.AddSeconds(_defaultSecondsToCache)
                       };
        }

    }
}
