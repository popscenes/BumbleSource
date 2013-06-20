using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using Website.Domain.Payment;

//using Website.Infrastructure.Service;

namespace PostaFlya.Domain.Binding
{
    public class DefaultServicesNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding DefaultServicesNinjectBinding");

            Bind<PaymentPackageServiceInterface>()
                .To<PaymentPackageService>()
                .InSingletonScope();

            var paymentServiveProvider = Kernel.Get<PaymentPackageServiceInterface>();
            paymentServiveProvider.Add(new CreditPaymentPackage(){Credits = 200, Payment = 2.00});
            paymentServiveProvider.Add(new CreditPaymentPackage() { Credits = 500, Payment = 5.00 });
            paymentServiveProvider.Add(new CreditPaymentPackage() { Credits = 1000, Payment = 10.00 });
            

            var kernel = Kernel as StandardKernel;
            

            Trace.TraceInformation("Finished Binding DefaultServicesNinjectBinding");

        }
    }
}
