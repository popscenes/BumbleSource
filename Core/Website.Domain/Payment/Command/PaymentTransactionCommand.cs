using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Domain.Payment.Command
{
    public class PaymentTransactionCommand : CommandInterface
    {
        public EntityInterface Entity { get; set; }
        public PaymentTransaction Transaction { get; set; }
        public string CommandId { get; set; }
        public PaymentPackageInterface Package { get; set; }
    }
}