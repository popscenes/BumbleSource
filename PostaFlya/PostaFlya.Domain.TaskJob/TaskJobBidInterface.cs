using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.TaskJob
{
    public static class TaskJobBidInterfaceExtensions
    {
        public static void CopyFieldsFrom(this TaskJobBidInterface target, TaskJobBidInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            target.BidAmount = source.BidAmount;
            target.BrowserId = source.BrowserId;
            target.TaskJobId = source.TaskJobId;
        }
    }

    public interface TaskJobBidInterface : EntityInterface
    {
        string TaskJobId { get; set; }
        string BrowserId { get; set; }
        double BidAmount { get; set; }
    }
}