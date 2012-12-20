using System;
using System.Collections.Specialized;
using System.Globalization;
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

        public PaypalExpressCheckout PaypalExpressCheckout { set; get; }      

        public Uri LaunchPaymentProcess(String paymentType, string enitityId, double amount)
        {
            var expressCHeckoutResult = PaypalExpressCheckout.SetExpressCheckout(amount.ToString(CultureInfo.InvariantCulture), paymentType, enitityId);
            if (PaypalExpressCheckout.IsValidResult(expressCHeckoutResult))
            {
                
            }

            var token = expressCHeckoutResult["TOKEN"];
            var strPaypalUrl = PaypalExpressCheckout.GetPayPalOrderUrl(token);

            return new Uri(strPaypalUrl);
        }


        public PaymentTransaction Processpayment(HttpRequestBase paymentDetails)
        {

            var expressCHeckoutResult = PaypalExpressCheckout.GetExpressCheckoutDetails(paymentDetails["token"]);
            if (!PaypalExpressCheckout.IsValidResult(expressCHeckoutResult))
            {

            }

            String paymentAmount = expressCHeckoutResult["PAYMENTREQUEST_0_AMT"];
            String payerId = expressCHeckoutResult["PAYERID"];

            var doExpressCheckoutResult = PaypalExpressCheckout.DoExpressCheckoutPayment(paymentDetails["token"], payerId, paymentAmount);
            if (!PaypalExpressCheckout.IsValidResult(doExpressCheckoutResult))
            {

            }

            String transactionId = expressCHeckoutResult["PAYMENTINFO_0_TRANSACTIONID"];
            String paymentType = expressCHeckoutResult["PAYMENTREQUEST_0_DESC"];
            String entityId = expressCHeckoutResult["PAYMENTREQUEST_0_INVNUM"];

            var transaction = new PaymentTransaction()
                {
                    AggregateId = entityId,
                    Amount = Double.Parse(paymentAmount),
                    BrowserId = entityId,
                    Message = "Transaction successfully completed",
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
