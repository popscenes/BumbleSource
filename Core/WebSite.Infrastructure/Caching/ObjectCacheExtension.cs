using System;
using System.Collections.Generic;
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

    public class DontCacheProvider : ObjectCache
    {
        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new List<KeyValuePair<string, object>>().GetEnumerator();
        }

        public override bool Contains(string key, string regionName = null)
        {
            return false;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            return value;
        }

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            return value;
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            return value;
        }

        public override object Get(string key, string regionName = null)
        {
            return null;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            return null;
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            return new Dictionary<string, object>();
        }

        public override object Remove(string key, string regionName = null)
        {
            return null;
        }

        public override long GetCount(string regionName = null)
        {
            return 0;
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return new DefaultCacheCapabilities();}
        }

        public override string Name
        {
            get { return "DontCache"; }
        }

        public override object this[string key]
        {
            get { return null; }
            set {  }
        }
    }
}