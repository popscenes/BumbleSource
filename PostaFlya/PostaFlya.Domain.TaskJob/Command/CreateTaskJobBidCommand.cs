namespace PostaFlya.Domain.TaskJob.Command
{
    public class CreateTaskJobBidCommand : Website.Domain.Command.DomainCommandBase
    {
        public string BrowserId { get; set; }
        public string TaskJobId { get; set; }
        public double BidAmount { get; set; }
    }
}