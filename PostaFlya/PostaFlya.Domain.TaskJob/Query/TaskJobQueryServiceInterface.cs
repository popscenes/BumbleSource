using System.Linq;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Domain.TaskJob.Query
{
    public interface TaskJobQueryServiceInterface : GenericQueryServiceInterface
    {
        IQueryable<TaskJobBidInterface> GetBids(string taskJobId);
    }
}