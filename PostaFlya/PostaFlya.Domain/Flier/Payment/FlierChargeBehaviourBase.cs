using Website.Domain.Browser;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Payment
{
    public abstract class FlierChargeBehaviourBase : EntityFeatureChargeBehaviourInterface
    {
        public abstract bool EnableOrDisableFeaturesBasedOnState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity) where EntityType : EntityInterface;

        public virtual EntityFeatureCharge GetChargeForAggregateMemberEntity<MemberEntityType>(EntityFeatureCharge entityFeatureCharge,
                                                                                       MemberEntityType entity) where MemberEntityType : EntityInterface
        {
            return null;//doesn't propagate
        }

        public virtual bool ChargeForState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity,
                                               GenericRepositoryInterface repository, GenericQueryServiceInterface queryService,
                                               CreditChargeServiceInterface creditPaymentService) where EntityType : EntityInterface
        {
            var flier = entity as FlierInterface;
            if (flier == null)
                return false;

            var chargableEntity = queryService.FindById<Browser>(flier.BrowserId);
            if (entityFeatureCharge.IsPaid || chargableEntity.AccountCredit < entityFeatureCharge.OutstandingBalance)
                return false;

            if (chargableEntity.AccountCredit < entityFeatureCharge.OutstandingBalance)
                return false;

            creditPaymentService.ChargeCreditsToEntity(entityFeatureCharge, flier, chargableEntity, repository);

            return true;
        }
    }
}