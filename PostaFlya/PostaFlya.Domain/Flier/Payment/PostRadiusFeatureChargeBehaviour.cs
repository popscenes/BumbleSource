using System;
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
                    Description = "UserContact",
                    CurrentStateMessage = "enabled",
                    Paid = 0,
                    BehaviourTypeString = typeof (LeadGenerationFeatureChargeBehaviour).FullName
                };
        }

        public void EnableOrDisableFeaturesBasedOnState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity) where EntityType : EntityInterface
        {
            var flier = entity as FlierInterface;
            if (flier == null)
                return;

            flier.Status = !entityFeatureCharge.IsPaid ? FlierStatus.PaymentPending : FlierStatus.Active;
        }

        public EntityFeatureCharge GetChargeForAggregateMemberEntity<MemberEntityType>(EntityFeatureCharge entityFeatureCharge,
                                                                                       MemberEntityType entity) where MemberEntityType : EntityInterface
        {
            return null;//doesn't propagate
        }

        public void ChargeForState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity,
                                               GenericRepositoryInterface repository, GenericQueryServiceInterface queryService) where EntityType : EntityInterface
        {
            var flier = entity as FlierInterface;
            if (flier == null)
                return;
            if (entityFeatureCharge.IsPaid || queryService.FindById<Browser>(flier.BrowserId).AccountCredit < entityFeatureCharge.OutstandingBalance)
                return;

            if(queryService.FindById<Browser>(flier.BrowserId).AccountCredit < entityFeatureCharge.OutstandingBalance)

            repository.UpdateEntity<Browser>(flier.BrowserId, browser =>
            {
                browser.AccountCredit -= entityFeatureCharge.OutstandingBalance;
            }); 

            entityFeatureCharge.Paid = entityFeatureCharge.Cost;
        }
    }
}