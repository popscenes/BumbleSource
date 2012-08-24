using System;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Command;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Behaviour.TaskJob.Command
{
    internal class CachedTaskJobRepository : BroadcastCachedRepository,
                                             TaskJobRepositoryInterface
    {
        private readonly TaskJobRepositoryInterface _taskJobRepository;

        public CachedTaskJobRepository([SourceDataSource]TaskJobRepositoryInterface taskJobRepository
            , ObjectCache cacheProvider           
            , CacheNotifier notifier)
            : base(cacheProvider, CachedTaskJobContext.Region, notifier, taskJobRepository)
        {
            _taskJobRepository = taskJobRepository;
        }

        public bool BidOnTask(TaskJobBidInterface bid)
        {
            var ret = _taskJobRepository.BidOnTask(bid);
            if (ret)
                this.InvalidateCachedData(GetKeyFor(CachedTaskJobContext.Bids, bid.TaskJobId));
            return ret;
        }

        public TaskJobBidInterface GetBidForUpdate(string taskJobId, string browserId)
        {
            return _taskJobRepository.GetBidForUpdate(taskJobId, browserId);
        }
    }
}