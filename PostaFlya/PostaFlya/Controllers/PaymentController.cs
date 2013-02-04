using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PostaFlya.Models;
using PostaFlya.Models.Flier;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Payment;
using Website.Domain.Browser;
using Website.Domain.Payment;
using Website.Domain.Payment.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentServiceProviderInterface _paymentServiceProvider;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BrowserInformationInterface _browserInfo;
        private readonly PaymentPackageServiceInterface _paymentPackageService;
        private readonly HttpContextBase _httpContext;
        private readonly CommandBusInterface _commandBus;


        public PaymentController(PaymentServiceProviderInterface paymentServiceProvider,
            CommandBusInterface commandBus,
            GenericQueryServiceInterface queryService,
            BrowserInformationInterface browserInfo,
            PaymentPackageServiceInterface paymentPackageService,
            HttpContextBase httpContext)
        {
            _paymentServiceProvider = paymentServiceProvider;
            _commandBus = commandBus;
            _queryService = queryService;
            _browserInfo = browserInfo;
            _paymentPackageService = paymentPackageService;
            _httpContext = httpContext;
        }


        public RedirectResult PaymentProcess(string paymentServiceName, double amount)
        {
            var paymentService = _paymentServiceProvider.GetPaymentServiceByName(paymentServiceName);
            return new RedirectResult(paymentService.LaunchPaymentProcess(PaymentType.AccountCredit.ToString(), _browserInfo.Browser.Id, amount).ToString());
        }

        public ViewResult PayPalSuccess(String token, String PayerID)
        {
            var paymentService = _paymentServiceProvider.GetPaymentServiceByName("paypal");
            var transaction = paymentService.Processpayment(_httpContext.Request);
            return PaymentCallback(transaction);

        }

        public ViewResult PayPalCancel()
        {
            var failedTransaction = new PaymentTransaction()
                {
                    Status = PaymentTransactionStatus.Fail,
                    Message = "Paypal Canceled",
                };

            //return RedirectToAction("PaymentCallback", new {transaction = failedTransaction});
            return PaymentCallback(failedTransaction);
        }

        public ViewResult PaymentCallback(PaymentTransaction transaction)
        {
            

            //if (transaction.Status == PaymentTransactionStatus.Fail)
            //{
            //    return View(viewModel);
            //}

            var browser = _queryService.FindById<Browser>(transaction.PaymentEntityId);
            var paymentPackage = _paymentPackageService.Get(transaction.Amount);

            transaction.Time = DateTimeOffset.UtcNow;
            var transactionCommand = new PaymentTransactionCommand()
                {
                    Entity = browser,
                    Transaction = transaction,
                    Package = paymentPackage
                };

            var paymentMessage = "";

            //what if this fails?
            var res = _commandBus.Send(transactionCommand) as MsgResponse;
            if ((res == null || res.IsError) && transaction.Status == PaymentTransactionStatus.Success)
            {
                //todo refund
                paymentMessage = "Failed to record transaction";
                Trace.TraceError("Failed to record payment transaction: \n{0}", JsonConvert.SerializeObject(transaction, Formatting.Indented));
            }       
            else if (transaction.Status == PaymentTransactionStatus.Success)
            {
                paymentMessage = ((CreditPaymentPackage) paymentPackage).Credits + " FLYA Credits for $" + transaction.Amount +
                                     " " + transaction.Message;
            }
            else
            {
                paymentMessage = transaction.Message;
            }

            var viewModel = new PaymentResult()
            {
                PaymentMessage = paymentMessage,
                Transaction = transaction
            };

            return View("PaymentCallback", viewModel);
        }

        public ViewResult PaymentTransactions()
        {
            var transactions = _queryService.FindAggregateEntities<PaymentTransaction>(_browserInfo.Browser.Id);
            return View(transactions.ToList());
        }

        public ActionResult AddAccountCredit()
        {
            var flierPaymentsModel = new FlierPaymentModel
                {
                    PaymentServiceList = _paymentServiceProvider.GetAllPaymentServices(),
                    PaymentOptions = _paymentPackageService.GetAll().Select(_ => _ as CreditPaymentPackage).ToList()
                };

            return View(flierPaymentsModel);
        }
    }
}