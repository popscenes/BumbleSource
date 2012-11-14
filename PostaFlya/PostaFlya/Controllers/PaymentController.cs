using System;
using System.Collections.Generic;
using System.Web.Mvc;
using PostaFlya.Models;
using Website.Application.Payment;

namespace PostaFlya.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentServiceProviderInterface _paymentServiceProviderInterface;


        public PaymentController(PaymentServiceProviderInterface paymentServiceProviderInterface)
        {
            _paymentServiceProviderInterface = paymentServiceProviderInterface;
        }

        public ActionResult FlierPayment(string id)
        {
            var flierPaymnetsModel = new FlierPaymentModel();

            var paymentServiceList = _paymentServiceProviderInterface.GetAllPaymentServices();
            flierPaymnetsModel.PaymentOptions = paymentServiceList as List<PaymentServiceInterface>;
            flierPaymnetsModel.FlierCost = 2.00;
            return View(flierPaymnetsModel);
        }

        public RedirectResult PaymentProcess(string paymentServiceName, string flierId, double amount)
        {
            var paymentService = _paymentServiceProviderInterface.GetPaymentServiceByName(paymentServiceName);
            return new RedirectResult(paymentService.LaunchPaymentProcess(PaymentType.Flier, flierId, amount).ToString());
        }

        public ViewResult PaymnetCallback(string paymnetid, string payerid, int entityId, double paymentAmount, string errorMessage)
        {
            throw new NotImplementedException();
        }

        public ViewResult PaymentTransactions()
        {
            throw new NotImplementedException();
        }
    }
}