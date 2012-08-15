using PostaFlya.Domain.TaskJob.Query;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.TaskJob.Command
{
    internal interface TaskJobRepositoryInterface : 
        GenericRepositoryInterface<TaskJobFlierBehaviourInterface>
    {
        bool BidOnTask(TaskJobBidInterface bid);
        TaskJobBidInterface GetBidForUpdate(string taskJobId, string browserId);
    }
}