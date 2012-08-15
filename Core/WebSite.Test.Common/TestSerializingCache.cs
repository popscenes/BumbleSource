using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using WebSite.Infrastructure.Util;

namespace WebSite.Test.Common
{
    public class TestSerializingCache : ObjectCache
    {

        Dictionary<string, byte[]> _serializedCache = new Dictionary<string, byte[]>(); 
        private const string RegionKeyTemplate = "{0}:{1}";

        public TestSerializingCache()
        {
        }

        public override string Name
        {
            get { return "TestSerializingCache"; }
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return DefaultCacheCapabilities.OutOfProcessProvider | DefaultCacheCapabilities.AbsoluteExpirations | DefaultCacheCapabilities.SlidingExpirations; }
        }

        public override object this[string key]
        {
            get
            {
                return this.Get(key, null);
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            return this.AddOrGetExisting(new CacheItem(key, value, regionName), policy).Value;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            return this.AddOrGetExisting(new CacheItem(key, value, regionName), new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }).Value;
        }

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            var data = this.Get(value.Key, value.RegionName);
            if (data != null)
            {
                return new CacheItem(value.Key, data, value.RegionName);
            }

            this.Set(value, policy);
            return value;
        }

        public override object Get(string key, string regionName = null)
        {
            byte[] data;
            if (!_serializedCache.TryGetValue(GetKey(key, regionName), out data))
                return null;

            return SerializeUtil.FromByteArray(data);
        }

        public override bool Contains(string key, string regionName = null)
        {
            return this.Get(key, regionName) != null;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            var data = this.Get(key, regionName);
            if (data != null)
            {
                return new CacheItem(key, data, regionName); ;
            }

            return null;
        }

        public override object Remove(string key, string regionName = null)
        {
            var data = this.Get(key, regionName);
            if (data != null)
            {
                if (regionName == null)
                    regionName = "";
                _serializedCache.Remove(GetKey(key, regionName));
            }

            return null;
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            this.Set(new CacheItem(key, value, regionName), policy);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            this.Set(new CacheItem(key, value, regionName), new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration });
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            if (item.Value == null) return;

            TimeSpan timeout;
            if (policy.SlidingExpiration != TimeSpan.Zero)
            {
                timeout = policy.SlidingExpiration;
            }
            else
            {
                timeout = policy.AbsoluteExpiration - DateTime.UtcNow;
            }

            var key = GetKey(item.Key, item.RegionName);
            _serializedCache[key] = SerializeUtil.ToByteArray(item.Value);
        }

        public override long GetCount(string regionName = null)
        {
            throw new NotSupportedException();
        }

        protected override IEnumerator<System.Collections.Generic.KeyValuePair<string, object>> GetEnumerator()
        {
            return
                _serializedCache.Select(
                    kv => new KeyValuePair<string, object>(kv.Key, SerializeUtil.FromByteArray(kv.Value))).GetEnumerator();
        }

        public override IDictionary<string, object> GetValues(System.Collections.Generic.IEnumerable<string> keys, string regionName = null)
        {
            throw new NotSupportedException();
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(System.Collections.Generic.IEnumerable<string> keys, string regionName = null)
        {
            throw new NotSupportedException();
        }

        private static string GetKey(string key, string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                return key;

            return string.Format(CultureInfo.InvariantCulture, RegionKeyTemplate, key, regionName);
         
        }
    }
}
