using System;
using System.Collections.ObjectModel;
using System.Runtime.Caching;

namespace Website.Infrastructure.Caching
{
    public static class ObjectCacheExtension
    {
        public static bool SupportsRegion(this ObjectCache cacheProvider)
        {
            return !(cacheProvider is MemoryCache);
        }
    }
    public abstract class CachedDataSourceBase
    {
        private readonly string _regionName;
        private readonly bool _regionSupported;

        protected CachedDataSourceBase(string regionName, bool regionSupported)
        {
            _regionSupported = regionSupported;
            _regionName = regionName;
        }

        public abstract ObjectCache Provider { get; }

        protected string GetKeyFor(string context, string id)
        {
            return context + ":" + id;
        }

        protected string GetInternalKey(string cacheKey)
        {
            return (!_regionSupported && _regionName != null) ? _regionName + ":" + cacheKey : cacheKey;
        }

        protected string GetRegion()
        {
            return (!_regionSupported) ? null : _regionName;
        }
    }
}