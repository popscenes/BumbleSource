using System;
using System.Collections.Generic;
using Website.Application.Domain.Payment;

namespace PostaFlya.Models
{
    public class FlierPaymentModel
    {
        public List<PaymentServiceInterface> PaymentOptions { get; set; }
    }
}