using System;

namespace Website.Application.Domain.Payment
{
    public class PaypalPaymentService: PaymentServiceInterface
    {
        public string PaymentServiceName
        {
            get { return "paypal"; }
            set
            {
                
            }
        }

        public Uri LaunchPaymentProcess(String paymentType, string enitityId, double amount)
        {
            throw new NotImplementedException();
        }
    }
}
