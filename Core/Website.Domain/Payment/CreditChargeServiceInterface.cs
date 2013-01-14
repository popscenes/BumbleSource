using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Domain.Payment
{
    public interface CreditChargeServiceInterface
    {
        bool ChargeCreditsToEntity(EntityFeatureCharge entityFeatureCharge, EntityInterface entity, ChargableEntityInterface chargeEntity, GenericRepositoryInterface repository);
    }
}