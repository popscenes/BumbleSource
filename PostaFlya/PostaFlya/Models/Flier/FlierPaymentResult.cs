using System;
using Website.Domain.Payment;

namespace PostaFlya.Models.Flier
{
    public class FlierPaymentResult
    {
        public String PaymentMessage { get; set; }

        public PaymentTransaction Transaction { get; set; }
    }
}