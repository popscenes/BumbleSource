using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Domain.Payment
{
    public class PaymentPackageService : PaymentPackageServiceInterface
    {
        private  Dictionary<double, PaymentPackageInterface> PaymentPackages = new Dictionary<double, PaymentPackageInterface>();
        public void Add(PaymentPackageInterface paymentPackage)
        {
            PaymentPackages.Add(paymentPackage.Payment, paymentPackage);
        }

        public PaymentPackageInterface Get(double payemnt)
        {
            return !PaymentPackages.Any(_ => Math.Abs(_.Key - payemnt) < 0.1) ? null : PaymentPackages.FirstOrDefault(_ => Math.Abs(_.Key - payemnt) < 0.1).Value;
        }

        public List<PaymentPackageInterface> GetAll()
        {
            return PaymentPackages.Select(_ => _.Value).ToList();
        }
    }
}
