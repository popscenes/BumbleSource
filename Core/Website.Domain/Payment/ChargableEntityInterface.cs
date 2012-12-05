using Website.Infrastructure.Domain;

namespace Website.Domain.Payment
{
    public interface ChargableEntityInterface  : EntityIdInterface
    {
        double AccountCredit { get; set; }
    }
}
