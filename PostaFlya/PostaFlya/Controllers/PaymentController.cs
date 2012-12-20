using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Models;
using PostaFlya.Models.Flier;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Payment;
using Website.Domain.Browser;
using Website.Domain.Payment;
using Website.Domain.Payment.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentServiceProviderInterface _paymentServiceProviderInterface;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly GenericRepositoryInterface _repositoryService;
        private readonly BrowserInformationInterface _browserInfo;
        private readonly PaymentPackageServiceInterface _paymentPackageService;
        private readonly HttpContextBase _httpContext;
        private readonly CommandBusInterface _commandBus;


        public PaymentController(PaymentServiceProviderInterface paymentServiceProviderInterface,
            CommandBusInterface commandBus,
            GenericQueryServiceInterface queryService,
            GenericRepositoryInterface repositoryService,
            BrowserInformationInterface browserInfo,
            PaymentPackageServiceInterface paymentPackageService,
            HttpContextBase httpContext)
        {
            _paymentServiceProviderInterface = paymentServiceProviderInterface;
            _commandBus = commandBus;
            _queryService = queryService;
            _repositoryService = repositoryService;
            _browserInfo = browserInfo;
            _paymentPackageService = paymentPackageService;
            _httpContext = httpContext;
        }


        public RedirectResult PaymentProcess(string paymentServiceName, double amount)
        {
            var paymentService = _paymentServiceProviderInterface.GetPaymentServiceByName(paymentServiceName);
            return new RedirectResult(paymentService.LaunchPaymentProcess(PaymentType.AccountCredit.ToString(), _browserInfo.Browser.Id, amount).ToString());
        }

        public ViewResult PayPalSuccess(String token, String PayerID)
        {
            var paymentService = _paymentServiceProviderInterface.GetPaymentServiceByName("paypal");
            var transaction = paymentService.Processpayment(_httpContext.Request);
            return PaymnetCallback(transaction);

        }

        public ViewResult PayPalCancel()
        {
            var failedTransaction = new PaymentTransaction()
                {
                    Status = PaymentTransactionStatus.Fail,
                    Message = "Paypal Canceled",
                };

            //return RedirectToAction("PaymnetCallback", new {transaction = failedTransaction});
            return PaymnetCallback(failedTransaction);
        }

        public ViewResult PaymnetCallback(PaymentTransaction transaction)
        {
            

            //if (transaction.Status == PaymentTransactionStatus.Fail)
            //{
            //    return View(viewModel);
            //}

            var browser = _queryService.FindById<Browser>(transaction.PaymentEntityId);
            var paymentPackage = _paymentPackageService.Get(transaction.Amount);
            
            var transactionCommand = new PaymentTransactionCommand()
                {
                    Entity = browser,
                    Transaction = transaction,
                    Package = paymentPackage
                };

            var res = _commandBus.Send(transactionCommand) as MsgResponse;

            var savedTransaction = _queryService.FindById<PaymentTransaction>(res.GetEntityId());

            var viewModel = new PaymentResult()
            {
                PaymentMessage = ((CreditPaymentPackage)paymentPackage).Credits + " for $" + transaction.Amount +  " " + transaction.Message,
                Transaction = transaction
            };

            return View("PaymnetCallback", viewModel);
        }

        public ViewResult PaymentTransactions()
        {
            var transactions = _queryService.FindAggregateEntities<PaymentTransaction>(_browserInfo.Browser.Id);
            return View(transactions.ToList());
        }

        public ActionResult AddAccountCredit()
        {
            var flierPaymnetsModel = new FlierPaymentModel
                {
                    PaymentServiceList = _paymentServiceProviderInterface.GetAllPaymentServices(),
                    PaymentOptions = _paymentPackageService.GetAll().Select(_ => _ as CreditPaymentPackage).ToList()
                };

            return View(flierPaymnetsModel);
        }
    }
}