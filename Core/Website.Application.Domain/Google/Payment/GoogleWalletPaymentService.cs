using System;
using Website.Application.Domain.Payment;
using Website.Application.Google.Payment;
using Website.Domain.Payment;
using Website.Infrastructure.Domain;

namespace Website.Application.Domain.Google.Payment
{
    public class GoogleWalletPaymentService : PaymentServiceInterface
    {
        public GoogleWalletDigitalGoods GoogleWalletDigitalGoods { get; set; }

        public string PaymentServiceName
        {
            get { return "googleWallet"; }
            set
            {

            }
        }

        public Uri LaunchPaymentProcess(string paymentType, string enitityId, double amount)
        {
            var digitalGoodsOrder = new DigitalGoodsOrder()
                {
                    Currency = GoogleWalletDigitalGoods.CurrencyCode.AUD,
                    Description = "Postaflya Payment",
                    Name = paymentType,
                    Price = amount,
                };

            digitalGoodsOrder.UserData.Add("browserId", enitityId);
            var jwt = GoogleWalletDigitalGoods.GenerateJWT(digitalGoodsOrder);
            return new Uri(GoogleWalletDigitalGoods.PaymentUrl + "?jwt=" + jwt);
        }

        public PaymentTransaction Processpayment(System.Web.HttpRequestBase paymentDetails)
        {
            var jwt = paymentDetails["jwt"];
            var digitalOrder = GoogleWalletDigitalGoods.OrderFromJWT(jwt);

            if (digitalOrder == null)
            {
                return new PaymentTransaction(){Status = PaymentTransactionStatus.Fail};
            }

            var transaction = new PaymentTransaction()
                {
                    Status = PaymentTransactionStatus.Success,
                    AggregateId = digitalOrder.UserData["browserId"],
                    Message = "Transaction successfully completed",
                    Amount = digitalOrder.Price,
                    PayerId = digitalOrder.UserData["browserId"],
                    PaymentEntityId = digitalOrder.UserData["browserId"],
                    TransactionId = digitalOrder.OrderId,
                    Id = digitalOrder.OrderId + "goog",
                    BrowserId = digitalOrder.UserData["browserId"]
                };

            transaction.SanitizeEntityId();

            return transaction;
        }
    }
}
