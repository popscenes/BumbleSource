using System;
using System.Collections.Generic;
using Website.Application.Domain.Payment;
using Website.Domain.Payment;

namespace PostaFlya.Models
{
    public class FlierPaymentModel
    {
        public List<CreditPaymentPackage> PaymentOptions { get; set; }
        public IList<PaymentServiceInterface> PaymentServiceList { get; set; }
    }
}