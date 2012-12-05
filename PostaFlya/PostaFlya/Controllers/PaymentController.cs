using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly CommandBusInterface _commandBus;


        public PaymentController(PaymentServiceProviderInterface paymentServiceProviderInterface,
            CommandBusInterface commandBus,
            GenericQueryServiceInterface queryService,
            GenericRepositoryInterface repositoryService,
            BrowserInformationInterface browserInfo)
        {
            _paymentServiceProviderInterface = paymentServiceProviderInterface;
            _commandBus = commandBus;
            _queryService = queryService;
            _repositoryService = repositoryService;
            _browserInfo = browserInfo;
        }


        public RedirectResult PaymentProcess(string paymentServiceName, string browserId, double amount)
        {
            var paymentService = _paymentServiceProviderInterface.GetPaymentServiceByName(paymentServiceName);
            return new RedirectResult(paymentService.LaunchPaymentProcess(PaymentType.AccountCredit.ToString(), browserId, amount).ToString());
        }

        public ViewResult PaymnetCallback(string paymnetId, string payerid, string entityId, double paymentAmount, string paymentType, string errorMessage)
        {
            var browser = _queryService.FindById<Browser>(entityId);
            var status = PaymentTransactionStatus.Fail;
            if (String.IsNullOrEmpty(errorMessage))
            {
                status = PaymentTransactionStatus.Success;
            }
            
            var transactionCommand = new PaymentTransactionCommand()
                {
                    Entity = browser,
                    PayerId = payerid,
                    PaymentId = paymnetId,
                    PaymentAmount = paymentAmount,
                    PaymentTransactionStatus = status,
                    PaymentType = (PaymentType)Enum.Parse(typeof(PaymentType), paymentType),
                    ErrorMessage = errorMessage
                };

            var res = _commandBus.Send(transactionCommand) as MsgResponse;

            var transaction = _queryService.FindById<PaymentTransaction>(res.GetEntityId());
            var viewModel = new FlierPaymentResult()
                {
                    PaymentMessage = errorMessage,
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