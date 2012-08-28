using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Website.Test.Common
{
    public class TestUtil
    {
        public static MemoryCache GetMemoryCache()
        {
            var cacheSettings = new NameValueCollection(3)
                                    {
                                        {"CacheMemoryLimitMegabytes", Convert.ToString(0)},
                                        {"physicalMemoryLimitPercentage", Convert.ToString(49)},
                                        {"pollingInterval", Convert.ToString("00:00:01")}
                                    };
            return new MemoryCache("TestCache", cacheSettings);
        }

        public static ObjectCache GetSerializingCache()
        {
            return new TestSerializingCache();
        }

        public static void ClearMemoryCache(ObjectCache objectCache)
        {
            var entires = objectCache.ToList();
            foreach (var keyValuePair in entires)
                objectCache.Remove(keyValuePair.Key);
        }
    }
}
