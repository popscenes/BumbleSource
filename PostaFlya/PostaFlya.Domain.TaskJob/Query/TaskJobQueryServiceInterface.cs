using System.Linq;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Domain.TaskJob.Query
{
    public interface TaskJobQueryServiceInterface : GenericQueryServiceInterface<TaskJobFlierBehaviourInterface>
    {
        IQueryable<TaskJobBidInterface> GetBids(string taskJobId);
    }
}