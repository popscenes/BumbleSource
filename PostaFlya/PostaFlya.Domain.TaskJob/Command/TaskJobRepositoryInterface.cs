using PostaFlya.Domain.TaskJob.Query;
using Website.Infrastructure.Command;

namespace PostaFlya.Domain.TaskJob.Command
{
    internal interface TaskJobRepositoryInterface : 
        GenericRepositoryInterface
    {
        bool BidOnTask(TaskJobBidInterface bid);
        TaskJobBidInterface GetBidForUpdate(string taskJobId, string browserId);
    }
}