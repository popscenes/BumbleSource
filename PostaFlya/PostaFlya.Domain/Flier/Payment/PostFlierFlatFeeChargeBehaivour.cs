using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.Domain.Properties;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Payment
{

    [Serializable]
    class PostFlierFlatFeeChargeBehaivour : FlierChargeBehaviourBase
    {
        public static readonly int costForPost = 100;
        public static EntityFeatureCharge GetPostRadiusFeatureCharge()
        {

            return new EntityFeatureCharge()
            {
                Cost = costForPost,
                Description = Resources.PostFlierFlatFeeChargeBehaivour_GetCharge_Description,
                CurrentStateMessage = Resources.PostFlierFlatFeeChargeBehaivour_CurrentStateMessage_Unpaid,
                Paid = 0,
                BehaviourTypeString = typeof(PostFlierFlatFeeChargeBehaivour).AssemblyQualifiedName
            };
        }

        public override bool IsFeatureEnabledBasedOnState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity)
        {
            var flier = entity as FlierInterface;
            if (flier == null)
                return false;

            entityFeatureCharge.CurrentStateMessage = !entityFeatureCharge.IsPaid ?
                Resources.PostRadiusFeatureChargeBehaviour_CurrentStateMessage_Unpaid :
                "";

            return entityFeatureCharge.IsPaid;
        }
    }
}
