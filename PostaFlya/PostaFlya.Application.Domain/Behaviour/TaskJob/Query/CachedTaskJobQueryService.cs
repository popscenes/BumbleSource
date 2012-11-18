using System.Linq;
using System.Runtime.Caching;
using Website.Application.Binding;
using Website.Application.Caching.Query;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Query;
using Website.Infrastructure.Caching.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Behaviour.TaskJob.Query
{
    public class CachedTaskJobQueryService : TimedExpiryCachedQueryService,
                                             TaskJobQueryServiceInterface
    {
        private readonly TaskJobQueryServiceInterface _taskJobQueryService;
        public CachedTaskJobQueryService([SourceDataSource]TaskJobQueryServiceInterface taskJobQueryService
                , ObjectCache cacheProvider
                , int defaultSecondsToCache = -1
                )
            : base(cacheProvider, taskJobQueryService, defaultSecondsToCache)
        {
            _taskJobQueryService = taskJobQueryService;
        }


        public IQueryable<TaskJobBidInterface> GetBids(string taskJobId)
        {
            return RetrieveCachedData(
                taskJobId.GetCacheKeyFor<TaskJobBid>("AggregateId"),
                () => _taskJobQueryService.GetBids(taskJobId).ToList())
                .AsQueryable();
        }

    }
}