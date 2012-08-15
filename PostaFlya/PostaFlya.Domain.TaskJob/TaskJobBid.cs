using System;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.TaskJob
{
    [Serializable]
    public class TaskJobBid : EntityBase<TaskJobBidInterface>, TaskJobBidInterface
    {
        public string TaskJobId { get; set; }
        public string BrowserId { get; set; }
        public double BidAmount { get; set; }
    }
}