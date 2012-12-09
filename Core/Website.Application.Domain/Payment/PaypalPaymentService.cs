using System;
using System.Collections.Specialized;
using System.Web;
using Website.Application.Intergrations.Payment;
using Website.Domain.Payment;

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
            var paypal = new PaypalExpressCheckout();
            var expressCHeckoutResult =  paypal.SetExpressCheckout(amount.ToString(), paymentType, enitityId);
            if (paypal.IsValidResult(expressCHeckoutResult))
            {
                
            }

            var token = expressCHeckoutResult["TOKEN"];
            var strPaypalUrl = paypal.GetPayPalOrderUrl(token);

            return new Uri(strPaypalUrl);
        }


        public PaymentTransaction Processpayment(HttpRequestBase PaymentDetails)
        {

            var paypal = new PaypalExpressCheckout();

            var expressCHeckoutResult = paypal.GetExpressCheckoutDetails(PaymentDetails["token"]);
            if (!paypal.IsValidResult(expressCHeckoutResult))
            {

            }

            String paymentAmount = expressCHeckoutResult["PAYMENTREQUEST_0_AMT"];
            String payerId = expressCHeckoutResult["PAYERID"];

            var doExpressCheckoutResult = paypal.DoExpressCheckoutPayment(PaymentDetails["token"], payerId, paymentAmount);
            if (!paypal.IsValidResult(doExpressCheckoutResult))
            {

            }

            String transactionId = doExpressCheckoutResult["PAYMENTINFO_0_TRANSACTIONID"];
            String paymentType = doExpressCheckoutResult["PAYMENTREQUEST_0_DESC"];
            String entityId = doExpressCheckoutResult["PAYMENTREQUEST_0_INVNUM"];

            var transaction = new PaymentTransaction()
                {
                    AggregateId = entityId,
                    Amount = Double.Parse(paymentAmount),
                    BrowserId = entityId,
                    Message = "",
                    Type = (PaymentType)Enum.Parse(typeof(PaymentType), paymentType),
                    Id = Guid.NewGuid().ToString(),
                    PaymentEntityId = entityId,
                    Status = PaymentTransactionStatus.Success,
                    TransactionId = transactionId,
                    PayerId = payerId,
                };

            return transaction;
        }
    }
}
