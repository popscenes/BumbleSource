using System;
using System.Runtime.Caching;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;

namespace Website.Infrastructure.Caching.Command
{
    public class CachedRepositoryBase : CachedDataSourceBase, GenericRepositoryInterface
    {
        private readonly ObjectCache _cacheProvider;
        private readonly GenericRepositoryInterface _genericRepository;

        public CachedRepositoryBase(ObjectCache cacheProvider, string regionName,
            [SourceDataSource]GenericRepositoryInterface genericRepository)
            : base(regionName, cacheProvider.SupportsRegion())//Because regions are not implemented MemoryCache in .NET Framework 4
        {
            if (cacheProvider == null)
            {
                throw new ArgumentNullException("cacheProvider");
            }

            _cacheProvider = cacheProvider;
            _genericRepository = genericRepository;
        }

        protected virtual void InvalidateCachedData(string cacheKey)
        {
            this._cacheProvider.Remove(GetInternalKey(cacheKey), GetRegion());
        }

        public override ObjectCache Provider
        {
            get { return _cacheProvider; }
        }

        public bool SaveChanges()
        {
            return _genericRepository.SaveChanges();
        }

        public virtual void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) where UpdateType : class, new()
        {
            Action<UpdateType> updateInvCacheAction
                = flier =>
                {
                    updateAction(flier);
                    this.InvalidateCachedData(GetKeyFor("entity", id));
                };
            _genericRepository.UpdateEntity(id, updateInvCacheAction);
        }

        public virtual void UpdateEntity(Type entity, string id, Action<object> updateAction)
        {
            Action<object> updateInvCacheAction
                = flier =>
                {
                    updateAction(flier);
                    this.InvalidateCachedData(GetKeyFor("entity", id));
                };
            _genericRepository.UpdateEntity(entity, id, updateInvCacheAction);
        }

        public virtual void Store<EntityType>(EntityType entity)
        {
            _genericRepository.Store(entity);
        }
    }
}
