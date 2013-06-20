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

            //behaviour factory
//            Bind<BehaviourFactoryInterface>().To<BehaviourFactory>().InSingletonScope();
//            Bind<Dictionary<FlierBehaviour, Type>>()
//                .ToSelf().InSingletonScope()
//                .WithMetadata("flierbehaviour", true);

//            //behaviour factory bindings
//            Kernel.Get<Dictionary<FlierBehaviour, Type>>(ctx => ctx.Has("flierbehaviour"))
//                .Add(FlierBehaviour.Default, typeof(FlierBehaviourInterface));
//            Bind<FlierBehaviourInterface>()
//                .To<FlierBehaviourDefault>()
//                .InSingletonScope();

            //just have a default non-functional repository for  FlierBehaviour.None
//            Bind<FlierBehaviourDefaultRespositoryInterface>()
//                .To<FlierBehaviourDefaultRespository>()
//                .InSingletonScope();
//            Bind<FlierBehaviourDefaultRespositoryInterface>()
//                .To<FlierBehaviourDefaultRespository>()
//                .InSingletonScope();

            Bind<PaymentPackageServiceInterface>()
                .To<PaymentPackageService>()
                .InSingletonScope();

            var paymentServiveProvider = Kernel.Get<PaymentPackageServiceInterface>();
            paymentServiveProvider.Add(new CreditPaymentPackage(){Credits = 200, Payment = 2.00});
            paymentServiveProvider.Add(new CreditPaymentPackage() { Credits = 500, Payment = 5.00 });
            paymentServiveProvider.Add(new CreditPaymentPackage() { Credits = 1000, Payment = 10.00 });
            

            var kernel = Kernel as StandardKernel;
            
            //generic services binding
//            Bind<GenericServiceFactoryInterface>()
//                .To<DefaultGenericServiceFactory>()
//                .InSingletonScope();

            Trace.TraceInformation("Finished Binding DefaultServicesNinjectBinding");

        }
    }
}
