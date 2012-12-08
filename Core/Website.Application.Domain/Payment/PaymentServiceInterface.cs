using System;
using System.Collections.Specialized;
using System.Web;
using Website.Domain.Payment;

namespace Website.Application.Domain.Payment
{
    public interface PaymentServiceInterface
    {
        String PaymentServiceName { get; set; }

        Uri LaunchPaymentProcess(String paymentType, String enitityId,  double amount);
        PaymentTransaction Processpayment(HttpRequestBase paymentDetails);
    }

    public class PaymentServiceTest : PaymentServiceInterface
    {
        public string PaymentServiceName { get; set; }


        public Uri LaunchPaymentProcess(String paymentType, string enitityId, double amount)
        {
 	        return new Uri("http://test.com/?amt=" + amount);
        }


        public PaymentTransaction Processpayment(HttpRequestBase paymentDetails)
        {
            return null;
        }
    }
}