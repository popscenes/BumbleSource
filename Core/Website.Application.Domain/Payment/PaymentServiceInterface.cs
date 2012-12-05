using System;

namespace Website.Application.Domain.Payment
{
    public interface PaymentServiceInterface
    {
        String PaymentServiceName { get; set; }

        Uri LaunchPaymentProcess(String paymentType, String enitityId,  double amount);
    }

    public class PaymentServiceTest : PaymentServiceInterface
    {
        public string PaymentServiceName { get; set; }


        public Uri LaunchPaymentProcess(String paymentType, string enitityId, double amount)
        {
 	        return new Uri("http://test.com/?amt=" + amount);
        }
    }
}