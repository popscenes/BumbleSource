//not using as yet, but if we decide to go down the path of using azure cach just bind this in instead of
//MemoryCache.Default
//this way the cache will be shared amoung instances. With MemoryCache.Default it'll be on each machine

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using Microsoft.ApplicationServer.Caching;

namespace Website.Application.Azure.Caching
{
    public class AzureCacheProvider : ObjectCache
    {
        private readonly DataCacheFactory _cacheFactory;

        private const string RegionKeyTemplate = "{0}:{1}";

        public AzureCacheProvider()
        {
            this._cacheFactory = new DataCacheFactory();
        }

        public void Clear()
        {
            _cacheFactory.GetDefaultCache().Clear();
        }

        public override string Name
        {
            get { return "AzureCacheService"; }
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
            var tryclearOnException = false;
            try
            {
                return _cacheFactory.GetDefaultCache().Get(GetKey(key, regionName));
            }
            catch (DataCacheException ex)
            {
                if (ex.ErrorCode == DataCacheErrorCode.RetryLater)
                {
                    // temporal failure, ignore and continue
                    Trace.TraceWarning("Azure Cache unavailable may have performance implications");
                    return null;

                }
                Trace.TraceError("Azure Cache error: {0} \n stack: {1} \n TrackingId: {2}", ex.Message, ex.StackTrace,
                                 ex.TrackingId);

            }
            catch (SerializationException exception)
            {
                Trace.TraceError("Azure Cache serialization error clearing cache: {0} \n stack: {1} \n ",
                                 exception.Message, exception.StackTrace);
                tryclearOnException = true;
            }
            catch (Exception exception)
            {
                Trace.TraceError("Azure Cache unexpected error : {0} \n stack: {1} \n Except {2}", 
                 exception.Message, exception.StackTrace, exception.ToString());
                tryclearOnException = true;
            }

            if (tryclearOnException)
            {
                try
                {
                    _cacheFactory.GetDefaultCache().Clear();
                }
                catch (Exception exception)
                {
                    Trace.TraceError("Failed to clear cache : {0} \n stack: {1} \n Excep {2}", exception.Message, exception.StackTrace, exception.ToString());
                }
            }


            return null;
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
                this._cacheFactory.GetDefaultCache().Remove(GetKey(key, regionName));
                return data;
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

            try
            {
                TimeSpan timeout;
                if (policy.SlidingExpiration != TimeSpan.Zero)
                {
                    timeout = policy.SlidingExpiration;
                }
                else
                {
                    timeout = policy.AbsoluteExpiration - DateTime.UtcNow;
                }

                this._cacheFactory.GetDefaultCache().Put(GetKey(item.Key, item.RegionName), item.Value, timeout);
            }
            catch (DataCacheException exception)
            {
                if (exception.ErrorCode == DataCacheErrorCode.RetryLater)
                {
                    // temporal failure, ignore and continue
                    return;
                }

                Trace.TraceError("Azure Cache unexpected error in store : {0} \n stack: {1} \n Except {2}",
                    exception.Message, exception.StackTrace, exception.ToString());
                
            }
            catch (Exception exception)
            {
                Trace.TraceError("Azure Cache unexpected error in store : {0} \n stack: {1} \n Except {2}",
                 exception.Message, exception.StackTrace, exception.ToString());
            }
        }

        public override long GetCount(string regionName = null)
        {
            throw new NotSupportedException();
        }

        protected override IEnumerator<System.Collections.Generic.KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotSupportedException();
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
            {
                return key;
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, RegionKeyTemplate, key, regionName);
            }
        }
    }
}
