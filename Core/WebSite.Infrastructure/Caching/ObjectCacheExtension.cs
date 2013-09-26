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
}