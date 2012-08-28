using System.Linq;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.TaskJob.Query
{
    public interface TaskJobQueryServiceInterface : GenericQueryServiceInterface
    {
        IQueryable<TaskJobBidInterface> GetBids(string taskJobId);
    }
}