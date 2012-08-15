using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Flier;

namespace PostaFlya.Models.Factory
{
    public interface FlierBehaviourViewModelFactoryInterface
    {
        BulletinFlierModel GetBulletinViewModel(FlierInterface flier, bool detailMode);
        DefaultDetailsViewModel GetBehaviourViewModel(FlierBehaviourInterface flierBehaviour);
    }
}