using System.Linq;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Query;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;
using WebSite.Infrastructure.Binding;

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
            : base(cacheProvider, CachedTaskJobContext.Region, defaultSecondsToCache)
        {
            _taskJobQueryService = taskJobQueryService;
        }

        public TaskJobFlierBehaviourInterface FindById(string id)
        {
            return RetrieveCachedData(
                GetKeyFor(CachedTaskJobContext.TaskJob, id),
                () => _taskJobQueryService.FindById(id));
        }

        public IQueryable<TaskJobBidInterface> GetBids(string taskJobId)
        {
            return RetrieveCachedData(
                GetKeyFor(CachedTaskJobContext.Bids, taskJobId),
                () => _taskJobQueryService.GetBids(taskJobId).ToList())
                .AsQueryable();
        }

        object QueryServiceInterface.FindById(string id)
        {
            return FindById(id);
        }
    }
}