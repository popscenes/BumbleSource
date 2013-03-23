using System;
using Website.Domain.Payment;

namespace PostaFlya.Models.Browser
{
    public class PaymentResult : PageModelInterface
    {
        public String PaymentMessage { get; set; }

        public PaymentTransaction Transaction { get; set; }
        public CreditPaymentPackage CreditPaymentPackage { get; set; }

        public String TransactionStatus { get; set; }

        public String SubHeading { get; set; }
        public string PageId { get; set; }
        public string ActiveNav { get; set; }
    }
}