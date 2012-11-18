using System;
using System.Runtime.Caching;
using Website.Application.Binding;
using Website.Application.Caching.Command;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using Website.Infrastructure.Caching.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Behaviour.TaskJob.Command
{
    internal class CachedTaskJobRepository : BroadcastCachedRepository,
                                             TaskJobRepositoryInterface
    {
        private readonly TaskJobRepositoryInterface _taskJobRepository;

        public CachedTaskJobRepository([SourceDataSource]TaskJobRepositoryInterface taskJobRepository
            , ObjectCache cacheProvider           
            , CacheNotifier notifier)
            : base(cacheProvider, notifier, taskJobRepository)
        {
            _taskJobRepository = taskJobRepository;
        }

        public bool BidOnTask(TaskJobBidInterface bid)
        {
            var ret = _taskJobRepository.BidOnTask(bid);
            if (ret)
                this.InvalidateCachedData(bid.TaskJobId.GetCacheKeyFor<TaskJobBid>("AggregateId"));
            return ret;
        }

        public TaskJobBidInterface GetBidForUpdate(string taskJobId, string browserId)
        {
            return _taskJobRepository.GetBidForUpdate(taskJobId, browserId);
        }
    }
}