using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ninject;
using Ninject.Syntax;

namespace Website.Infrastructure.Query
{
    public interface QueryOptionsInterface
    {
        QueryOptionsInterface CacheFor(TimeSpan span);
        QueryOptionsInterface SkipCache();
        QueryOptionsInterface ClearCache();
    }

    public interface QueryChannelInterface
    {
        ReturnType Query<ReturnType, QueryType>(QueryType query, ReturnType defaultRet = default(ReturnType),
            Action<QueryOptionsInterface> queryOptions = null)
            where QueryType : QueryInterface;
    }

//    public static class QueryChannelExtensions
//    {
//        public static IEnumerable<ReturnType> PerformParrallel<ReturnType, QueryType>
//            (this QueryChannelInterface queryChannel, IEnumerable<QueryType> queries, ReturnType def = default(ReturnType)
//             , Action<QueryOptionsInterface> queryOptions = null) where QueryType : QueryInterface
//        {
//            var ret = new ConcurrentQueue<ReturnType>();
//            Parallel.ForEach(queries, query => 
//                ret.Enqueue(queryChannel.Query(query, def, queryOptions)));
//            return ret.ToArray();
//        }
//    }

    public class DefaultQueryChannel : QueryChannelInterface
    {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly ObjectCache _objectCache;

        public DefaultQueryChannel(IResolutionRoot resolutionRoot, ObjectCache objectCache)
        {
            _resolutionRoot = resolutionRoot;
            _objectCache = objectCache;
        }

        public ReturnType Query<ReturnType, QueryType>(QueryType query, ReturnType defaultRet = default(ReturnType),
            Action<QueryOptionsInterface> queryOptions = null) 
            where QueryType : QueryInterface
        {

            var handler = _resolutionRoot.Get<QueryHandlerInterface<QueryType, ReturnType>>();

            ReturnType ret;

            using (var customize = new QueryOptions(_objectCache))
            {
                if (queryOptions != null)
                    queryOptions(customize);

                if (customize.TryGetCached(query, out ret))
                    return ret;

                ret = handler.Query(query);
                customize.ApplyCustomisation(query, ret);
            }

            return (Equals(ret, default(ReturnType)))
                       ? defaultRet
                       : ret;
        }
    }

    class QueryOptions : QueryOptionsInterface, IDisposable
    {
        private readonly ObjectCache _objectCache;
        private TimeSpan? _cacheTimeSpan;
        private bool _skipCache;
        private bool _clearCache;

        public QueryOptions(ObjectCache objectCache)
        {
            _objectCache = objectCache;
            _skipCache = false;
            _clearCache = false;
        }

        public QueryOptionsInterface CacheFor(TimeSpan span)
        {
            _cacheTimeSpan = span;
            return this;
        }

        public QueryOptionsInterface SkipCache()
        {
            _skipCache = true;
            return this;
        }

        public QueryOptionsInterface ClearCache()
        {
            _clearCache = true;
            return this;
        }

        public void Dispose()
        {
        }

        public void ApplyCustomisation<ReturnType>(QueryInterface query, ReturnType value)
        {
            if (_cacheTimeSpan == null && !_clearCache) return;

            var key = query.GetCacheKey<ReturnType>();

            if (_clearCache && !_cacheTimeSpan.HasValue)
                _objectCache.Remove(key);

            if (_cacheTimeSpan.HasValue)
                _objectCache.Add(key, value, GetCachePolicyFor(_cacheTimeSpan));

        }

        public bool TryGetCached<ReturnType>(QueryInterface query, out ReturnType ret)
        {
            var key = query.GetCacheKey<ReturnType>();
            if (_skipCache || _clearCache || !_objectCache.Contains(key))
            {
                ret = default(ReturnType);
                return false;
            }

            ret = (ReturnType)_objectCache.Get(key);

            return true;
        }

        private DateTimeOffset GetCachePolicyFor(TimeSpan? cacheTimeSpan)
        {
            return DateTimeOffset.Now + cacheTimeSpan.Value;
        }
    }

    public static class QueryInterfaceExtensions
    {
        private static readonly JsonSerializerSettings Settings
            = new JsonSerializerSettings()
            {
                ReferenceLoopHandling =
                    ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects
            };

        public static string GetCacheKey<ReturnType>(this QueryInterface query)
        {
            //if you get exception add [JsonIgnore] to properties causing it
            return JsonConvert.SerializeObject(query, Settings) + ":" + typeof(ReturnType).FullName;
        }
    }
}