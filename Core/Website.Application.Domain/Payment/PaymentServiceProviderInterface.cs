using System.Collections.Generic;

namespace Website.Application.Domain.Payment
{
    public interface PaymentServiceProviderInterface
    {
        void Add(PaymentServiceInterface paymentService);
        IList<PaymentServiceInterface> GetAllPaymentServices();
        PaymentServiceInterface GetPaymentServiceByName(string name);
    }
}
