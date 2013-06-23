using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Infrastructure.Caching.Query
{
    public static class CachedExtensions
    {
        public static string GetCacheKeyFor(this string id, Type type, string context)
        {
            return type.FullName + ":" + context  + ":" + id;
        }

        public static string GetCacheKeyFor<EntityType>(this string id, string context)
        {
            return id.GetCacheKeyFor(typeof(EntityType), context);
        }

    }
    public abstract class CachedQueryServiceBase
        : GenericQueryServiceInterface
    {
        private readonly ObjectCache _cacheProvider;
        private readonly GenericQueryServiceInterface _genericQueryService;

        protected CachedQueryServiceBase(ObjectCache cacheProvider
            , GenericQueryServiceInterface genericQueryService) 
        {
            _cacheProvider = cacheProvider;
            _genericQueryService = genericQueryService;
        }

        public virtual ObjectCache Provider
        {
            get { return _cacheProvider; }
        }


        protected T RetrieveCachedData<T>(string cacheKey
            , Func<T> fallbackFunction
            , Func<T, bool> shouldCache = null
            , CacheItemPolicy cachePolicy = null) where T : class
        {
            T data;
            try
            {
                data = this._cacheProvider.Get(cacheKey) as T;
                if (data != null)
                {
                    return data;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning("RetrieveCachedData failed for key: {0}, region:{1} \n message: {2}\n stack: {3}", cacheKey, "", e.Message, e.StackTrace);
                try
                {
                    this._cacheProvider.Remove(cacheKey);
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning("RetrieveCachedData attempt to delete key after retrieve exception, key: {0}, region:{1} \n message: {2}\n stack: {3}", cacheKey, "", exception.Message, e.StackTrace);
                }
            }

            data = fallbackFunction();
            if (data != null && (shouldCache == null || shouldCache(data)))
            {
                if (cachePolicy == null)
                    cachePolicy = GetDefaultPolicy();
                this._cacheProvider.Add(new CacheItem(cacheKey, data), cachePolicy);
            }

            return data;
        }

        protected abstract CacheItemPolicy GetDefaultPolicy();
        
        public EntityRetType FindById<EntityRetType>(string id) where EntityRetType : class, AggregateRootInterface, new()
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                id.GetCacheKeyFor<EntityRetType>("Id"),
                () => _genericQueryService.FindById<EntityRetType>(id));
        }

        public object FindById(Type entity, string id)
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                id.GetCacheKeyFor(entity, "Id"),
                () => _genericQueryService.FindById(entity, id));
        }

        public EntityRetType FindByAggregate<EntityRetType>(string id, string aggregateRootId) where EntityRetType : class, AggregateInterface, new()
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                id.GetCacheKeyFor<EntityRetType>("Id"),
                () => _genericQueryService.FindByAggregate<EntityRetType>(id, aggregateRootId));
        }

        public object FindByAggregate(Type entity, string id, string aggregateRootId)
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                id.GetCacheKeyFor(entity, "Id"),
                () => _genericQueryService.FindByAggregate(entity, id, aggregateRootId));
        }

        public IQueryable<string> FindAggregateEntityIds<EntityRetType>(string aggregateRootId)
            where EntityRetType : class, AggregateInterface, new()
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                aggregateRootId.GetCacheKeyFor<EntityRetType>("AggregateId"),
                () => _genericQueryService.FindAggregateEntityIds<EntityRetType>(aggregateRootId)
                .ToList()).AsQueryable();
        }

        public IQueryable<string> GetAllIds<EntityRetType>() where EntityRetType : class, AggregateRootInterface, new()
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                "".GetCacheKeyFor<EntityRetType>("AllIds"),
                () => _genericQueryService.GetAllIds<EntityRetType>()
                .ToList()).AsQueryable();
        }

        public IQueryable<string> GetAllIds(Type type)
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                "".GetCacheKeyFor(type, "AllIds"),
                () => _genericQueryService.GetAllIds(type)
                .ToList()).AsQueryable();
        }

        public IQueryable<AggregateInterface> GetAllAggregateIds<EntityRetType>() where EntityRetType : class, AggregateInterface, new()
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                "".GetCacheKeyFor<EntityRetType>("AllIds"),
                () => _genericQueryService.GetAllAggregateIds<EntityRetType>()
                .ToList()).AsQueryable();
        }
    }
}
