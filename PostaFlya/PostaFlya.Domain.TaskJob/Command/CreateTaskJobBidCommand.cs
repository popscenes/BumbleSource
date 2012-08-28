using Website.Infrastructure.Command;

namespace PostaFlya.Domain.TaskJob.Command
{
    public class CreateTaskJobBidCommand : DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public string TaskJobId { get; set; }
        public double BidAmount { get; set; }
    }
}