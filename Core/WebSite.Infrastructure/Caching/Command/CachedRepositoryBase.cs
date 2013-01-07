using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Caching.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Infrastructure.Caching.Command
{
    public class CachedRepositoryBase : GenericRepositoryInterface
    {
        private readonly ObjectCache _cacheProvider;
        private readonly GenericRepositoryInterface _genericRepository;
        private HashSet<object> _entitiesForInvalidation = new HashSet<object>();  

        public CachedRepositoryBase(ObjectCache cacheProvider,
            [SourceDataSource]GenericRepositoryInterface genericRepository)
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
            this._cacheProvider.Remove(cacheKey);
        }

        public virtual ObjectCache Provider
        {
            get { return _cacheProvider; }
        }

        public bool SaveChanges()
        {
            var ret = _genericRepository.SaveChanges();
            foreach (var entity in _entitiesForInvalidation)
            {
                InvalidateEntity(entity);
            }
            _entitiesForInvalidation.Clear();
            return ret;

        }

        public virtual void UpdateEntity<UpdateType>(string id
            , Action<UpdateType> updateAction) where UpdateType : class, EntityIdInterface, new()
        {
            Action<UpdateType> updateInvCacheAction
                = entity =>
                    {
                        InvalidateEntity(entity);//invalidate before state change just in case  not 100% effective but
                        //only matters for things like changing aggregate root or friendly id which shouldn't happen
                        //considering removing
                        updateAction(entity);
                        _entitiesForInvalidation.Remove(entity);
                        _entitiesForInvalidation.Add(entity);
                    };
            _genericRepository.UpdateEntity(id, updateInvCacheAction);
        }

        public virtual void UpdateEntity(Type entityTyp, string id, Action<object> updateAction)
        {
            Action<object> updateInvCacheAction
                = entity =>
                    {
                        InvalidateEntity(entity);//invalidate before state change just in case  not 100% effective but
                                                //only matters for things like changing aggregate root or friendly id which shouldn't happen
                                                //considering removing
                        updateAction(entity);
                        _entitiesForInvalidation.Remove(entity);
                        _entitiesForInvalidation.Add(entity);
                    };
            _genericRepository.UpdateEntity(entityTyp, id, updateInvCacheAction);
        }

        public virtual void Store<EntityType>(EntityType entity)
        {
            InvalidateEntity(entity);
            _genericRepository.Store(entity);
            _entitiesForInvalidation.Remove(entity);
            _entitiesForInvalidation.Add(entity);
        }

        protected virtual void InvalidateEntity(object entity)
        {
            var entitiesIn = new HashSet<object>();
            AggregateMemberEntityAttribute.GetAggregateEnities(entitiesIn, entity);
            foreach (var ent in entitiesIn.OfType<EntityIdInterface>())
            {
                this.InvalidateCachedData(ent.FriendlyId.GetCacheKeyFor(ent.GetType(), "FriendlyId"));
                this.InvalidateCachedData(ent.Id.GetCacheKeyFor(ent.GetType(), "Id"));
                this.InvalidateCachedData("".GetCacheKeyFor(ent.GetType(), "AllIds"));
                var hasRoot = ent as AggregateInterface;
                if(hasRoot != null)
                    this.InvalidateCachedData(hasRoot.AggregateId.GetCacheKeyFor(ent.GetType(), "AggregateId"));
            }
        }
    }
}
