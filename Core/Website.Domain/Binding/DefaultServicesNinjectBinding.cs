using System.Diagnostics;
using Ninject.Modules;
using Website.Domain.Location;
using Website.Domain.Payment;

//using Website.Infrastructure.Service;

namespace Website.Domain.Binding
{
    public class DefaultServicesNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding DefaultServicesNinjectBinding");
            //location service  
            Bind<LocationServiceInterface>().To<LocationService>().InSingletonScope();
            Bind<CreditChargeServiceInterface>().To<CreditChargeService>().InSingletonScope();

            Trace.TraceInformation("Finished Binding DefaultServicesNinjectBinding");

        }
    }
}
