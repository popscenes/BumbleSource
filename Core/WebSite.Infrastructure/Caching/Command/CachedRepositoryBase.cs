using System;
using System.Runtime.Caching;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Caching.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

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

        public virtual void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) where UpdateType : class, EntityIdInterface, new()
        {
            Action<UpdateType> updateInvCacheAction
                = entity =>
                {
                    this.InvalidateCachedData(GetKeyFor(CachedQueryServiceBase.FriendlyIdPrefix(typeof(UpdateType)), entity.FriendlyId));
                    this.InvalidateCachedData(GetKeyFor(CachedQueryServiceBase.IdPrefix(typeof(UpdateType)), entity.Id));
                    updateAction(entity);
                    
                };
            _genericRepository.UpdateEntity(id, updateInvCacheAction);
        }

        public virtual void UpdateEntity(Type entityTyp, string id, Action<object> updateAction)
        {
            Action<object> updateInvCacheAction
                = entity =>
                    {
                        var asEntityId = entity as EntityIdInterface;
                        if (asEntityId != null)
                            this.InvalidateCachedData(GetKeyFor(CachedQueryServiceBase.FriendlyIdPrefix(entityTyp), asEntityId.FriendlyId));
                        this.InvalidateCachedData(GetKeyFor(CachedQueryServiceBase.IdPrefix(entityTyp), id));
                        updateAction(entity);             
                    };
            _genericRepository.UpdateEntity(entityTyp, id, updateInvCacheAction);
        }

        public virtual void Store<EntityType>(EntityType entity)
        {
            _genericRepository.Store(entity);
        }
    }
}
