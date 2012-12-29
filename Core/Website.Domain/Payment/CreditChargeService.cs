using System;
using Website.Infrastructure.Command;

namespace Website.Domain.Payment
{
    public class CreditChargeService : CreditChargeServiceInterface
    {
        public CreditChargeService()
        {
            
        }

        public bool ChargeCreditsToEntity(EntityFeatureCharge entityFeatureCharge, ChargableEntityInterface entity, GenericRepositoryInterface repository)
        {

            repository.UpdateEntity(entity.GetType(), entity.Id, e =>
                {
                    var chargeable = e as ChargableEntityInterface;
                    if (chargeable != null) chargeable.AccountCredit -= entityFeatureCharge.OutstandingBalance;
                });

            entityFeatureCharge.Paid = entityFeatureCharge.Cost;

            var creditTrandaction = new CreditTransaction() 
                { Id = Guid.NewGuid().ToString(), 
                    AggregateId = entity.Id,
                    AggregateTypeTag = entity.GetType().ToString(), 
                    CreditTransactionType = entityFeatureCharge.Description,
                    Credits = entityFeatureCharge.Cost
                };
            repository.Store<CreditTransaction>(creditTrandaction);

            return true;
        }
    }
}