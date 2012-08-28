using Website.Infrastructure.Command;
using Website.Domain.Location;

namespace PostaFlya.Domain.TaskJob.Command
{
    public class TaskJobBehaviourCreateCommand : DefaultCommandBase
    {
        public Locations ExtraLocations;
        public string BrowserId { get; set; }
        public string FlierId { get; set; }
        public double MaxAmount { get; set; }
        public double CostOverhead { get; set; }
    }
}