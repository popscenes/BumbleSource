using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using PostaFlya.Application.Domain.Query;
using PostaFlya.Domain.Browser;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Query;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Likes;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;
using WebSite.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Flier.Query
{
    public class CachedFlierQueryService : CachedQueryServiceWithBrowser 
                                           , FlierQueryServiceInterface
    {
        private readonly FlierQueryServiceInterface _queryService;

        public CachedFlierQueryService([SourceDataSource]FlierQueryServiceInterface queryService
                , ObjectCache cacheProvider
                , int defaultSecondsToCache = -1)
            : base(cacheProvider, CachedFlierContext.Region, queryService, defaultSecondsToCache)
        {
            _queryService = queryService;
        }

//        public IQueryable<LikeInterface> GetLikes(string id)
//        {
//            return RetrieveCachedData(
//                GetKeyFor(CachedFlierContext.Likes, id),
//                () => this._queryService.GetLikes(id).ToList())
//                .AsQueryable();
//        }
//
//        public IQueryable<LikeInterface> GetLikesByBrowser(string browserId)
//        {
//            return RetrieveCachedData(
//                GetKeyFor(CachedFlierContext.Likes, browserId),
//                () => this._queryService.GetLikesByBrowser(browserId).ToList())
//                .AsQueryable();
//        }
//
//        public IQueryable<CommentInterface> GetComments(string id, int take)
//        {
//            return RetrieveCachedData(
//                GetKeyFor(CachedFlierContext.Comments, id),
//                () => this._queryService.GetComments(id).ToList())
//                .AsQueryable();
//        }

        public IList<string> FindFliersByLocationTagsAndDistance(PostaFlya.Domain.Location.Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0)
        {
            return _queryService.FindFliersByLocationTagsAndDistance(location, tags, distance, take, sortOrder, skip);
        }

    }
}