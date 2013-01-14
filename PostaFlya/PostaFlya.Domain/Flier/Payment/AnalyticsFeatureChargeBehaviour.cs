using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.Domain.Properties;
using Website.Domain.Browser;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Payment
{
    public class AnalyticsFeatureChargeBehaviour : FlierChargeBehaviourBase
    {
        public static EntityFeatureCharge GetAnalyticsFeatureChargeBehaviour()
        {
            return new EntityFeatureCharge()
            {
                Cost = 200,
                Description = Resources.AnalyticsFeatureChargeBehaviour_GetAnalyticsFeatureChargeBehaviour,
                CurrentStateMessage = "",
                Paid = 0,
                BehaviourTypeString = typeof(AnalyticsFeatureChargeBehaviour).AssemblyQualifiedName
            };
        }

        public override bool EnableOrDisableFeaturesBasedOnState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity)
        {
            var flier = entity as FlierInterface;
            if (flier == null)
                return false;

            entityFeatureCharge.CurrentStateMessage = !entityFeatureCharge.IsPaid ?
                Resources.AnalyticsFeatureChargeBehaviour_EnableOrDisableFeaturesBasedOnState_Not_Paid :
                "";

            var orig = flier.Status;
            flier.Status = !entityFeatureCharge.IsPaid ? FlierStatus.PaymentPending : FlierStatus.Active;
            return orig != flier.Status;
        }

    }
}
