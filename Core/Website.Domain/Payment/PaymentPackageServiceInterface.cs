using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Domain.Payment
{
    public interface PaymentPackageServiceInterface
    {
        void Add(PaymentPackageInterface paymentPackage);
        PaymentPackageInterface Get(double payemnt);
        List<PaymentPackageInterface> GetAll();
    }
}
