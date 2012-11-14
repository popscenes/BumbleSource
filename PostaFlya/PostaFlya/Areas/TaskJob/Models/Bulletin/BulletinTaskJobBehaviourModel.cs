using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.TaskJob;

namespace PostaFlya.Areas.TaskJob.Models.Bulletin
{
    public static class TaskJobFlierBehaviourInterfaceExtensions
    {
        public static BulletinTaskJobBehaviourModel ToBulletinTaskJobViewModel(this FlierInterface flier)
        {
            return BulletinTaskJobBehaviourModel.FromFlierProperties(flier);
        }
    }

    public class BulletinTaskJobBehaviourModel
    {
        [Display(Name = "CostOverheadDisplay", ResourceType = typeof(Properties.Resources))]
        public double CostOverhead { get; set; }

        public static BulletinTaskJobBehaviourModel FromFlierProperties(FlierInterface flier)
        {
            var properties = new TaskJobFlierBehaviourFlierProperties(flier.ExtendedProperties);

            return new BulletinTaskJobBehaviourModel()
                       {
                           CostOverhead = properties.CostOverhead
                       };
        } 
    }
}