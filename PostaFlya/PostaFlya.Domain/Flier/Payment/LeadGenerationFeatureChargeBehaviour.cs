using System;
using PostaFlya.Domain.Properties;
using Website.Domain.Browser;
using Website.Domain.Claims;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Payment
{
    public class LeadGenerationFeatureChargeBehaviour : EntityFeatureChargeBehaviourInterface
    {
        public static EntityFeatureCharge GetLeadGenerationFeatureCharge()
        {
            return new EntityFeatureCharge()
                {
                    Cost = 400,
                    Description = "UserContact",
                    CurrentStateMessage = "enabled",
                    Paid = 0,
                    BehaviourTypeString = typeof(LeadGenerationFeatureChargeBehaviour).FullName
                };
        }

        public void EnableOrDisableFeaturesBasedOnState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity) where EntityType : EntityInterface
        {
            var claim = entity as ClaimInterface;
            if (claim == null || !claim.ClaimContext.Contains(FlierInterfaceExtensions.ClaimContextSendUserDetails))
                return;
            claim.ClaimContext = entityFeatureCharge.IsPaid ? 
                FlierInterfaceExtensions.ClaimContextSendUserDetailsEnabled : 
                FlierInterfaceExtensions.ClaimContextSendUserDetailsDisabled;

            entityFeatureCharge.CurrentStateMessage = !entityFeatureCharge.IsPaid ? 
                Resources.LeadGenerationFeatureChargeBehaviour_LeadContactUnavailableUntilPaidFor : "";
        }

        public EntityFeatureCharge GetChargeForAggregateMemberEntity<MemberEntityType>(EntityFeatureCharge entityFeatureCharge,
                                                                                       MemberEntityType entity) where MemberEntityType : EntityInterface
        {
            var claim = entity as ClaimInterface;
            return claim == null ? null : GetLeadGenerationFeatureCharge();
        }

        public void ChargeForState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity,
                                               GenericRepositoryInterface repository, GenericQueryServiceInterface queryService) where EntityType : EntityInterface
        {
            var claim = entity as ClaimInterface;
            if (claim == null || entityFeatureCharge.IsPaid)
                return;

            var flier = queryService.FindById<Flier>(claim.AggregateId);
            if (flier == null)
                return;

            if (!claim.ClaimContext.Contains(FlierInterfaceExtensions.ClaimContextSendUserDetails)
                || queryService.FindById<Browser>(flier.BrowserId).AccountCredit < entityFeatureCharge.OutstandingBalance) return;
    
            repository.UpdateEntity<Browser>(flier.BrowserId, browser =>
                {
                    browser.AccountCredit -= entityFeatureCharge.OutstandingBalance;                     
                });

            entityFeatureCharge.Paid = entityFeatureCharge.Cost;
        }
    }
}