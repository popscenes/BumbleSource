using Website.Infrastructure.Command;

namespace Website.Domain.Payment
{
    public interface CreditChargeServiceInterface
    {
        bool ChargeCreditsToEntity(EntityFeatureCharge entityFeatureCharge, ChargableEntityInterface entity,
                                   GenericRepositoryInterface repository);
    }
}