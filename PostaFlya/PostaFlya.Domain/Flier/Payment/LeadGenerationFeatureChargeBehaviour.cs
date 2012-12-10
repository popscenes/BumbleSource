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
                    Cost = 500,
                    Description = Resources.LeadGenerationFeatureChargeBehaviour_Description,
                    CurrentStateMessage = "",
                    Paid = 0,
                    BehaviourTypeString = typeof(LeadGenerationFeatureChargeBehaviour).AssemblyQualifiedName
                };
        }

        public bool EnableOrDisableFeaturesBasedOnState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity) where EntityType : EntityInterface
        {
            var flier = entity as FlierInterface;
            if (flier != null)
            {
                entityFeatureCharge.CurrentStateMessage = Resources.LeadGenerationFeatureChargeBehaviour_CurrentStateMessage_Flier_LeadGenerationStatus;
                return false;
            }

            var claim = entity as ClaimInterface;
            if (claim == null || !claim.ClaimContext.Contains(FlierInterfaceExtensions.ClaimContextSendUserDetails))
                return false;
            var orig = claim.ClaimContext;
            claim.ClaimContext = entityFeatureCharge.IsPaid ? 
                FlierInterfaceExtensions.ClaimContextSendUserDetailsEnabled : 
                FlierInterfaceExtensions.ClaimContextSendUserDetailsDisabled;

            entityFeatureCharge.CurrentStateMessage = !entityFeatureCharge.IsPaid ? 
                Resources.LeadGenerationFeatureChargeBehaviour_LeadContactUnavailableUntilPaidFor : "";
            return !claim.ClaimContext.Equals(orig);
        }

        public EntityFeatureCharge GetChargeForAggregateMemberEntity<MemberEntityType>(EntityFeatureCharge entityFeatureCharge,
                                                                                       MemberEntityType entity) where MemberEntityType : EntityInterface
        {
            var claim = entity as ClaimInterface;
            return claim == null ? null : GetLeadGenerationFeatureCharge();
        }

        public bool ChargeForState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity, GenericRepositoryInterface repository, GenericQueryServiceInterface queryService) where EntityType : EntityInterface
        {
            var claim = entity as ClaimInterface;
            if (claim == null || entityFeatureCharge.IsPaid)
                return false;
             
            var flier = queryService.FindById<Flier>(claim.AggregateId);
            if (flier == null)
                return false;

            if (!claim.ClaimContext.Contains(FlierInterfaceExtensions.ClaimContextSendUserDetails)
                || queryService.FindById<Browser>(flier.BrowserId).AccountCredit < entityFeatureCharge.OutstandingBalance) 
                return false;
    
            repository.UpdateEntity<Browser>(flier.BrowserId, browser =>
                {
                    browser.AccountCredit -= entityFeatureCharge.OutstandingBalance;                     
                });

            entityFeatureCharge.Paid = entityFeatureCharge.Cost;
            return true;
        }
    }
}