using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Application.Domain.Payment
{
    public class PaymentServiceProvider : PaymentServiceProviderInterface
    {
        private Dictionary<String, PaymentServiceInterface> paymentServices = new Dictionary<string, PaymentServiceInterface>();
        public void Add(PaymentServiceInterface paymentService)
        {
            paymentServices.Add(paymentService.PaymentServiceName, paymentService);
        }

        public IList<PaymentServiceInterface> GetAllPaymentServices()
        {
            return paymentServices.Values.ToList();
        }

        public PaymentServiceInterface GetPaymentServiceByName(string name)
        {
            return paymentServices.First(p => p.Key.Equals(name)).Value;
        }
    }
}
