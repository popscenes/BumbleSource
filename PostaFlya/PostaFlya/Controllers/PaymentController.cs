using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Models;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Flier;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Payment;
using PostaFlya.Domain.Browser;
using Website.Domain.Payment;
using Website.Domain.Payment.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentServiceProviderInterface _paymentServiceProvider;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly PostaFlyaBrowserInformationInterface _browserInfo;
        private readonly PaymentPackageServiceInterface _paymentPackageService;
        private readonly ConfigurationServiceInterface _configurationServiceInterface;
        private readonly HttpContextBase _httpContext;
        private readonly MessageBusInterface _messageBus;


        public PaymentController(PaymentServiceProviderInterface paymentServiceProvider,
            MessageBusInterface messageBus,
            GenericQueryServiceInterface queryService,
            PostaFlyaBrowserInformationInterface browserInfo,
            PaymentPackageServiceInterface paymentPackageService,
            ConfigurationServiceInterface configurationServiceInterface,
            HttpContextBase httpContext)
        {
            _paymentServiceProvider = paymentServiceProvider;
            _messageBus = messageBus;
            _queryService = queryService;
            _browserInfo = browserInfo;
            _paymentPackageService = paymentPackageService;
            _configurationServiceInterface = configurationServiceInterface;
            _httpContext = httpContext;
        }


        public RedirectResult PaymentProcess(string paymentServiceName, double amount)
        {
            var paymentService = _paymentServiceProvider.GetPaymentServiceByName(paymentServiceName);
            return new RedirectResult(paymentService.LaunchPaymentProcess(PaymentType.AccountCredit.ToString(), _browserInfo.Browser.Id, amount).ToString());
        }

        public ViewResult GoogleWallet(String jwt)
        {
            ViewBag.Jwt = jwt;
            if (_configurationServiceInterface != null)
            {
                ViewBag.GoogleJsLink = _configurationServiceInterface.GetSetting("GooglePaymentJs");
            }
            return View();
        }

        public ActionResult  GoogleWalletCallback()
        {
            var paymentService = _paymentServiceProvider.GetPaymentServiceByName("googleWallet");
            var transaction = paymentService.Processpayment(_httpContext.Request);
            transaction.Time = DateTimeOffset.UtcNow;
            var browser = _queryService.FindById<Browser>(transaction.PaymentEntityId);
            var paymentPackage = _paymentPackageService.Get(transaction.Amount);

            var transactionCommand = new PaymentTransactionCommand()
            {
                Entity = browser,
                Transaction = transaction,
                Package = paymentPackage
            };

            _messageBus.Send(transactionCommand);
            
            Response.Write(transaction.TransactionId);
            return new HttpStatusCodeResult(200);
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
            _messageBus.Send(transactionCommand);
            if (transaction.Status == PaymentTransactionStatus.Success)
            {
                paymentMessage = ((CreditPaymentPackage) paymentPackage).Credits + " FLYA Credits for $" + transaction.Amount +
                                     " " + transaction.Message;
            }
            else
            {
                paymentMessage = transaction.Message;
            }

            return PaymentDone(paymentMessage, true);
        }

        public ViewResult PaymentDone(String message, Boolean success)
        {
            var viewModel = new PaymentResult()
            {
                PaymentMessage = message,
                PageId = WebConstants.ProfileCreditPage,
                ActiveNav = WebConstants.ProfileNavPayment
            };

            if (success)
            {
                viewModel.TransactionStatus = "Complete";
                viewModel.SubHeading = "THANK YOU FOR ADDING FLYA CREDITS";
            }
            else
            {
                viewModel.TransactionStatus = "Failed";
                viewModel.SubHeading = "Unfortunetly No Credits were added";
            }

            return View("PaymentCallback", viewModel);
        }



        public ViewResult PaymentTransactions()
        {

            var transactions = _queryService.FindAggregateEntities<PaymentTransaction>(_browserInfo.Browser.Id);
            return View(new PaymentTrasactionPageModel()
                {
                    PageId = WebConstants.ProfileTransactionPage,
                    ActiveNav = WebConstants.ProfileNavPayment,
                    Transactions = transactions.Select(_ => _.ToViewModel()).ToList()
                });
        }

        public ActionResult AddAccountCredit()
        {
            var flierPaymentsModel = new FlierPaymentModel
                {
                    PaymentServiceList = _paymentServiceProvider.GetAllPaymentServices(),
                    PaymentOptions = _paymentPackageService.GetAll().Select(_ => _ as CreditPaymentPackage).ToList(),
                    PageId = WebConstants.ProfileCreditPage,
                    ActiveNav = WebConstants.ProfileNavPayment
                };

            return View(flierPaymentsModel);
        }
    }
}