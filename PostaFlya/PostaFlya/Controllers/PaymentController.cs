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
        private readonly HttpContextBase _httpContext;
        private readonly CommandBusInterface _commandBus;


        public PaymentController(PaymentServiceProviderInterface paymentServiceProviderInterface,
            CommandBusInterface commandBus,
            GenericQueryServiceInterface queryService,
            GenericRepositoryInterface repositoryService,
            BrowserInformationInterface browserInfo,
            HttpContextBase httpContext)
        {
            _paymentServiceProviderInterface = paymentServiceProviderInterface;
            _commandBus = commandBus;
            _queryService = queryService;
            _repositoryService = repositoryService;
            _browserInfo = browserInfo;
            _httpContext = httpContext;
        }


        public RedirectResult PaymentProcess(string paymentServiceName, string browserId, double amount)
        {
            var paymentService = _paymentServiceProviderInterface.GetPaymentServiceByName(paymentServiceName);
            return new RedirectResult(paymentService.LaunchPaymentProcess(PaymentType.AccountCredit.ToString(), browserId, amount).ToString());
        }

        public ViewResult PayPalSuccess()
        {
            var paymentService = _paymentServiceProviderInterface.GetPaymentServiceByName("paypal");
            var transaction = paymentService.Processpayment(_httpContext.Request);
            return PaymnetCallback(transaction);

        }

        //public ViewResult PayPalCancel()
        //{

        //}

        public ViewResult PaymnetCallback(PaymentTransaction transaction)
        {
            var browser = _queryService.FindById<Browser>(transaction.PaymentEntityId);
            
            var transactionCommand = new PaymentTransactionCommand()
                {
                    Entity = browser,
                    Transaction = transaction
                };

            var res = _commandBus.Send(transactionCommand) as MsgResponse;

            var savedTransaction = _queryService.FindById<PaymentTransaction>(res.GetEntityId());
            var viewModel = new FlierPaymentResult()
                {
                    PaymentMessage = transaction.Message,
                    Transaction = transaction
                };
            return View(viewModel);
        }

        public ViewResult PaymentTransactions()
        {
            var transactions = _queryService.FindAggregateEntities<PaymentTransaction>(_browserInfo.Browser.Id);
            return View(transactions.ToList());
        }

        public ActionResult AddAccountCredit()
        {
            var flierPaymnetsModel = new FlierPaymentModel();

            var paymentServiceList = _paymentServiceProviderInterface.GetAllPaymentServices();
            flierPaymnetsModel.PaymentOptions = paymentServiceList as List<PaymentServiceInterface>;
            return View(flierPaymnetsModel);
        }
    }
}