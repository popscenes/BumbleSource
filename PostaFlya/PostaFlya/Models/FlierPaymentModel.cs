using System;
using System.Collections.Generic;
using Website.Application.Payment;

namespace PostaFlya.Models
{
    public class FlierPaymentModel
    {
        public List<PaymentServiceInterface> PaymentOptions { get; set; }
    }
}