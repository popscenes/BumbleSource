using System;
using Website.Domain.Payment;

namespace PostaFlya.Models.Flier
{
    public class PaymentResult
    {
        public String PaymentMessage { get; set; }

        public PaymentTransaction Transaction { get; set; }
        public CreditPaymentPackage CreditPaymentPackage { get; set; }
    }
}