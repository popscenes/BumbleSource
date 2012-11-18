using System.Collections.Generic;
using PostaFlya.Domain.Behaviour;
using Website.Infrastructure.Domain;
using Website.Domain.Location;

namespace PostaFlya.Domain.TaskJob
{
    public static class TaskJobFlierBehavourInterfaceExtensions
    {
        public static void CopyFieldsFrom(this TaskJobFlierBehaviourInterface target, TaskJobFlierBehaviourInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            target.Flier = source.Flier;
            target.MaxAmount = source.MaxAmount;
            target.CostOverhead = source.CostOverhead;
            target.ExtraLocations = source.ExtraLocations;
            target.Bids = source.Bids != null ? new HashSet<TaskJobBid>(source.Bids) : null;
        }
    }

    public interface TaskJobFlierBehaviourInterface : FlierBehaviourInterface
    {
        double MaxAmount { get; set; }
        double CostOverhead { get; set; }
        Locations ExtraLocations { get; set; }
        HashSet<TaskJobBid> Bids { get; set; } 
    }
}