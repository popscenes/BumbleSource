using System;
using System.Runtime.Caching;

namespace WebSite.Infrastructure.Caching.Command
{
    public class CachedRepositoryBase : CachedDataSourceBase
    {
        private readonly ObjectCache _cacheProvider;

        public CachedRepositoryBase(ObjectCache cacheProvider, string regionName)
            : base(regionName, cacheProvider.SupportsRegion())//Because regions are not implemented MemoryCache in .NET Framework 4
        {
            if (cacheProvider == null)
            {
                throw new ArgumentNullException("cacheProvider");
            }

            _cacheProvider = cacheProvider;
        }

        protected virtual void InvalidateCachedData(string cacheKey)
        {
            this._cacheProvider.Remove(GetInternalKey(cacheKey), GetRegion());
        }

        public override ObjectCache Provider
        {
            get { return _cacheProvider; }
        }

    }
}
