using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Website.Application.Binding;
using Website.Application.Caching.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Domain.Location;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Infrastructure.Binding;
using Website.Application.Domain.Query;
using Website.Domain.Tag;

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

        public IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, string board = null, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0)
        {
            return _queryService.FindFliersByLocationTagsAndDistance(location, tags, board, distance, take, sortOrder, skip);
        }

    }
}