using System;
using System.Web.Mvc;

namespace PostaFlya.Controllers
{
    public class PaymentController
    {
        public ViewResult FlierPayment(string id)
        {
            throw new NotImplementedException();
        }

        public RedirectResult PaymentProcess(string id, string flier, double d)
        {
            throw new NotImplementedException();
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