using System;

namespace Website.Application.Payment
{
    public enum PaymentType
    {
        Flier
    }
    public interface PaymentServiceInterface
    {
        String PaymentServiceName { get; set; }

        Uri LaunchPaymentProcess(PaymentType paymentType, String enitityId,  double amount);
    }

    public class PaymentServiceTest : PaymentServiceInterface
    {
        public string PaymentServiceName { get; set; }


        public Uri LaunchPaymentProcess(PaymentType paymentType, string enitityId, double amount)
        {
 	        return new Uri("http://test.com/?amt=" + amount);
        }
    }
}