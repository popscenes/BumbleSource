using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using Ninject.Syntax;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Behaviour.Command;
using PostaFlya.Domain.Behaviour.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Service;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
//using Website.Infrastructure.Service;

namespace PostaFlya.Domain.Binding
{
    public class DefaultServicesNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding DefaultServicesNinjectBinding");

            //behaviour factory
            Bind<BehaviourFactoryInterface>().To<BehaviourFactory>().InSingletonScope();
            Bind<Dictionary<FlierBehaviour, Type>>()
                .ToSelf().InSingletonScope()
                .WithMetadata("flierbehaviour", true);
            Bind<FlierBehaviourQueryServiceInterface>()
                .To<DefaultFlierBehaviourQueryService>();

            //behaviour factory bindings
            Kernel.Get<Dictionary<FlierBehaviour, Type>>(ctx => ctx.Has("flierbehaviour"))
                .Add(FlierBehaviour.Default, typeof(FlierBehaviourInterface));
            Bind<FlierBehaviourInterface>()
                .To<FlierBehaviourDefault>()
                .InSingletonScope();

            //just have a default non-functional repository for  FlierBehaviour.None
            Bind<FlierBehaviourDefaultRespositoryInterface>()
                .To<FlierBehaviourDefaultRespository>()
                .InSingletonScope();
            Bind<FlierBehaviourDefaultRespositoryInterface>()
                .To<FlierBehaviourDefaultRespository>()
                .InSingletonScope();

            var kernel = Kernel as StandardKernel;
            
            //generic services binding
//            Bind<GenericServiceFactoryInterface>()
//                .To<DefaultGenericServiceFactory>()
//                .InSingletonScope();

            Trace.TraceInformation("Finished Binding DefaultServicesNinjectBinding");

        }
    }
}
