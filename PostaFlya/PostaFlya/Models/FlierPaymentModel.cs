using System;
using System.Collections.Generic;
using Website.Application.Domain.Payment;
using Website.Domain.Payment;

namespace PostaFlya.Models
{
    public class FlierPaymentModel : PageModelInterface
    {
        public List<CreditPaymentPackage> PaymentOptions { get; set; }
        public IList<PaymentServiceInterface> PaymentServiceList { get; set; }
        public string PageId { get; set; }
        public string ActiveNav { get; set; }
    }
}