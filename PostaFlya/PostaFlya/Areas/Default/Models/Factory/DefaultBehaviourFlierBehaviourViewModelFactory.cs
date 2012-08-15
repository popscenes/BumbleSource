using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;

namespace PostaFlya.Areas.Default.Models.Factory
{
    public class DefaultBehaviourFlierBehaviourViewModelFactory : FlierBehaviourViewModelFactoryInterface
    {
        public BulletinFlierModel GetBulletinViewModel(FlierInterface flier, bool detailMode)
        {
            var defaultret = flier.ToViewModel<BulletinBehaviourModel>(detailMode);
            defaultret.Behaviour = new BulletinBehaviourModel();
            return defaultret;
        }

        public DefaultDetailsViewModel GetBehaviourViewModel(FlierBehaviourInterface flierBehaviour)
        {
            return new DefaultDetailsViewModel()
            {
                Flier = GetBulletinViewModel(flierBehaviour.Flier, true)
            };
        }
    }
}