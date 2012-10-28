using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Infrastructure.Caching.Query
{
    public abstract class CachedQueryServiceBase
        : CachedDataSourceBase, GenericQueryServiceInterface
    {
        private readonly ObjectCache _cacheProvider;
        private readonly GenericQueryServiceInterface _genericQueryService;

        protected CachedQueryServiceBase(ObjectCache cacheProvider
            , string regionName
            , [SourceDataSource]GenericQueryServiceInterface genericQueryService) 
            : base(regionName, cacheProvider.SupportsRegion())
        {
            _cacheProvider = cacheProvider;
            _genericQueryService = genericQueryService;
        }

        public override ObjectCache Provider
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
                data = this._cacheProvider.Get(GetInternalKey(cacheKey), GetRegion()) as T;
                if (data != null)
                {
                    return data;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning("RetrieveCachedData failed for key: {0}, region:{1} \n message: {2}\n stack: {3}", cacheKey, GetRegion(), e.Message, e.StackTrace);
                try
                {
                    this._cacheProvider.Remove(GetInternalKey(cacheKey), GetRegion());
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning("RetrieveCachedData attempt to delete key after retrieve exception, key: {0}, region:{1} \n message: {2}\n stack: {3}", cacheKey, GetRegion(), exception.Message, e.StackTrace);
                }
            }

            data = fallbackFunction();
            if (data != null && (shouldCache == null || shouldCache(data)))
            {
                if (cachePolicy == null)
                    cachePolicy = GetDefaultPolicy();
                this._cacheProvider.Add(new CacheItem(GetInternalKey(cacheKey), data, GetRegion()), cachePolicy);
            }

            return data;
        }

        protected abstract CacheItemPolicy GetDefaultPolicy();
        
        public EntityRetType FindById<EntityRetType>(string id) where EntityRetType : class, new()
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                GetKeyFor(IdPrefix(typeof(EntityRetType)), id),
                () => _genericQueryService.FindById<EntityRetType>(id));
        }

        public object FindById(Type entity, string id)
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                GetKeyFor(IdPrefix(entity), id),
                () => _genericQueryService.FindById(entity, id));
        }

        public EntityRetType FindByFriendlyId<EntityRetType>(string id) where EntityRetType : class, new()
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                GetKeyFor(FriendlyIdPrefix(typeof(EntityRetType)), id),
                () => _genericQueryService.FindByFriendlyId<EntityRetType>(id));
        }

        public object FindByFriendlyId(Type entity, string id)
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                GetKeyFor(FriendlyIdPrefix(entity), id),
                () => _genericQueryService.FindByFriendlyId(entity, id));
        }

        public IQueryable<string> FindAggregateEntityIds<EntityRetType>(string aggregateRootId, int take = -1)
            where EntityRetType : class, AggregateInterface, new()
        {
            if (_genericQueryService == null)
                return null;
            return RetrieveCachedData(
                GetKeyFor(AggregateTakePrefix(typeof(EntityRetType), take), aggregateRootId),
                () => _genericQueryService.FindAggregateEntityIds<EntityRetType>(aggregateRootId, take)
                .ToList()).AsQueryable();
        }

        public static string IdPrefix(Type entityTyp)
        {
            return entityTyp.FullName + " id";
        }

        public static string FriendlyIdPrefix(Type entityTyp)
        {
            return entityTyp.FullName + " friendlyId";
        }

        public static string AggregateTakePrefix(Type entityTyp, int take)
        {
            return entityTyp.FullName + "agg take:" + take;
        }
    }
}
