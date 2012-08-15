using System;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Areas.TaskJob.Models.Bulletin;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;

namespace PostaFlya.Areas.TaskJob.Models.Factory
{
    public class TaskJobBehaviourFlierBehaviourViewModelFactory : FlierBehaviourViewModelFactoryInterface
    {
        public BulletinFlierModel GetBulletinViewModel(FlierInterface flier, bool detailMode)
        {
            var ret = flier.ToViewModel<BulletinTaskJobBehaviourModel>(detailMode);
            ret.Behaviour = flier.ToBulletinTaskJobViewModel();
            return ret;
        }

        public DefaultDetailsViewModel GetBehaviourViewModel(FlierBehaviourInterface flierBehaviour)
        {
            var behaviour = flierBehaviour as TaskJobFlierBehaviourInterface;
            return new TaskJobDetailsViewModel()
            {
                Flier = GetBulletinViewModel(flierBehaviour.Flier, true),
                CostOverhead = behaviour != null ? behaviour.CostOverhead : 0
            };
        }
    }
}