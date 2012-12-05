using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Domain.Payment.Command
{
    public class PaymentTransactionCommand : CommandInterface
    {
        public EntityInterface Entity { get; set; }

        public string PayerId { get; set; }

        public string PaymentId { get; set; }

        public double PaymentAmount { get; set; }
        public string CommandId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string ErrorMessage { get; set; }
        public PaymentTransactionStatus PaymentTransactionStatus { get; set; }
    }
}