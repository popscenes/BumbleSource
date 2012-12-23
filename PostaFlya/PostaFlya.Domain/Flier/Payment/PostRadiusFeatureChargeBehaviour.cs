using System;
using PostaFlya.Domain.Properties;
using Website.Domain.Browser;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Payment
{
    public class PostRadiusFeatureChargeBehaviour : EntityFeatureChargeBehaviourInterface
    {
        public static readonly int RatePerSqKm = 1; 
        public static EntityFeatureCharge GetPostRadiusFeatureCharge(int extendPostRadius)
        {
            var init = (int)((5 + extendPostRadius)*(5 + extendPostRadius)*3.14*RatePerSqKm);
            var cost = init + 5 - (init%5);
            
            return new EntityFeatureCharge()
                {
                    Cost = cost,
                    Description = Resources.PostRadiusFeatureChargeBehaviour_GetPostRadiusFeatureCharge_Description,
                    CurrentStateMessage = Resources.PostRadiusFeatureChargeBehaviour_CurrentStateMessage_Unpaid,
                    Paid = 0,
                    BehaviourTypeString = typeof(PostRadiusFeatureChargeBehaviour).AssemblyQualifiedName
                };
        }

        public bool EnableOrDisableFeaturesBasedOnState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity) where EntityType : EntityInterface
        {
            var flier = entity as FlierInterface;
            if (flier == null)
                return false;

            entityFeatureCharge.CurrentStateMessage = !entityFeatureCharge.IsPaid ?
                Resources.PostRadiusFeatureChargeBehaviour_CurrentStateMessage_Unpaid : 
                "";

            var orig = flier.Status;
            flier.Status = !entityFeatureCharge.IsPaid ? FlierStatus.PaymentPending : FlierStatus.Active;
            return orig != flier.Status;
        }

        public EntityFeatureCharge GetChargeForAggregateMemberEntity<MemberEntityType>(EntityFeatureCharge entityFeatureCharge,
                                                                                       MemberEntityType entity) where MemberEntityType : EntityInterface
        {
            return null;//doesn't propagate
        }

        public bool ChargeForState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity, GenericRepositoryInterface repository, GenericQueryServiceInterface queryService, CreditChargeServiceInterface creditPaymentService) where EntityType : EntityInterface
        {
            var flier = entity as FlierInterface;
            if (flier == null)
                return false;

            var chargableEntity = queryService.FindById<Browser>(flier.BrowserId);
            if (entityFeatureCharge.IsPaid || chargableEntity.AccountCredit < entityFeatureCharge.OutstandingBalance)
                return false;

            if (chargableEntity.AccountCredit < entityFeatureCharge.OutstandingBalance)
                return false;

            creditPaymentService.ChargeCreditsToEntity(entityFeatureCharge, chargableEntity, repository);
            /*
            repository.UpdateEntity<Browser>(flier.BrowserId, browser =>
            {
                browser.AccountCredit -= entityFeatureCharge.OutstandingBalance;
            }); 

            entityFeatureCharge.Paid = entityFeatureCharge.Cost;*/
            return true;
        }
    }
}