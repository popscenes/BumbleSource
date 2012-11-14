using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Application.Payment
{
    public interface PaymentServiceProviderInterface
    {
        void Add(PaymentServiceInterface paymentService);
        IList<PaymentServiceInterface> GetAllPaymentServices();
        PaymentServiceInterface GetPaymentServiceByName(string name);
    }
}
