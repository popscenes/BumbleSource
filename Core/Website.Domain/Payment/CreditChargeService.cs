using System;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Domain.Payment
{
    public class CreditChargeService : CreditChargeServiceInterface
    {
        public CreditChargeService()
        {
            
        }

        public bool ChargeCreditsToEntity(EntityFeatureCharge entityFeatureCharge, EntityInterface entity, ChargableEntityInterface chargeEntity, GenericRepositoryInterface repository)
        {

            repository.UpdateEntity(chargeEntity.GetType(), chargeEntity.Id, e =>
                {
                    var chargeable = e as ChargableEntityInterface;
                    if (chargeable != null) chargeable.AccountCredit -= entityFeatureCharge.OutstandingBalance;
                });

            entityFeatureCharge.Paid = entityFeatureCharge.Cost;

            var creditTrandaction = new CreditTransaction() 
                { 
                    Id = Guid.NewGuid().ToString(), 
                    AggregateId = chargeEntity.Id,
                    AggregateTypeTag = chargeEntity.GetType().ToString(), 
                    CreditTransactionType = entityFeatureCharge.Description,
                    Credits = entityFeatureCharge.Cost,
                    EntityIdForCharge = entity.Id,
                    EntityTypeTagForCharge = entity.GetType().ToString()
                };
            repository.Store<CreditTransaction>(creditTrandaction);

            return true;
        }
    }
}