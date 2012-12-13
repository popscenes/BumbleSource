using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Domain.Payment
{
    public class CreditPaymentPackage : PaymentPackageInterface
    {
        public Double Payment { get; set; }
        public Int32 Credits { get; set; }
    }

    public interface PaymentPackageInterface
    {
        Double Payment { get; set; }
    }
}
