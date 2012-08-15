using System;
using System.Diagnostics;
using System.Runtime.Caching;

namespace WebSite.Infrastructure.Caching.Query
{
    public abstract class CachedQueryServiceBase
        : CachedDataSourceBase
    {
        private readonly ObjectCache _cacheProvider;

        protected CachedQueryServiceBase(ObjectCache cacheProvider, string regionName) 
            : base(regionName, cacheProvider.SupportsRegion())
        {
            _cacheProvider = cacheProvider;
        }

        public override ObjectCache Provider
        {
            get { return _cacheProvider; }
        }


        protected T RetrieveCachedData<T>(string cacheKey
            , Func<T> fallbackFunction
            , Func<T, bool> shouldCache = null
            , CacheItemPolicy cachePolicy = null) where T : class
        {
            T data;
            try
            {
                data = this._cacheProvider.Get(GetInternalKey(cacheKey), GetRegion()) as T;
                if (data != null)
                {
                    return data;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning("RetrieveCachedData failed for key: {0}, region:{1} \n message: {2}\n stack: {3}", cacheKey, GetRegion(), e.Message, e.StackTrace);
                try
                {
                    this._cacheProvider.Remove(GetInternalKey(cacheKey), GetRegion());
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning("RetrieveCachedData attempt to delete key after retrieve exception, key: {0}, region:{1} \n message: {2}\n stack: {3}", cacheKey, GetRegion(), exception.Message, e.StackTrace);
                }
            }

            data = fallbackFunction();
            if (data != null && (shouldCache == null || shouldCache(data)))
            {
                if (cachePolicy == null)
                    cachePolicy = GetDefaultPolicy();
                this._cacheProvider.Add(new CacheItem(GetInternalKey(cacheKey), data, GetRegion()), cachePolicy);
            }

            return data;
        }

        protected abstract CacheItemPolicy GetDefaultPolicy();
    }
}
