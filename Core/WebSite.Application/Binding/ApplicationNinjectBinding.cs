using System;
using System.Diagnostics;
using System.Linq;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using WebSite.Application.Command;
using WebSite.Application.Communication;
using WebSite.Infrastructure.Binding;
using WebSite.Infrastructure.Command;
using WebSite.Application.WebsiteInformation;

namespace WebSite.Application.Binding
{
    public class ApplicationNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationNinjectBinding");

           
            //command handlers
            var kernel = Kernel as StandardKernel;
            kernel.BindCommandHandlersFromCallingAssembly(c => c.InTransientScope());

            Kernel.Bind<WebsiteInfoServiceInterface>().To<CachedWebsiteInfoService>();

            //broadcast communicator
            Kernel.Bind<BroadcastCommunicatorFactoryInterface>()
                .To<DefaultBroadcastCommunicatorFactory>()
                .InSingletonScope();

            Bind<BroadcastCommunicatorInterface>()
                .ToMethod(ctx =>
                {
                    var idFunc = ctx.Kernel.Get<Func<string>>(metadata => metadata.Has("BroadcastCommunicator"));
                    var endpoint = idFunc();
                    var fact = ctx.Kernel.Get<BroadcastCommunicatorFactoryInterface>();
                    return fact.GetCommunicatorForEndpoint(endpoint);
                })
                .WithMetadata("BroadcastCommunicator", true);

            Bind<QueuedCommandScheduler>()
                .ToMethod(ctx =>
                {
                    var idFunc = ctx.Kernel.Get<Func<string>>(metadata => metadata.Has("BroadcastCommunicator"));
                    var endpoint = idFunc();
                    var fact = ctx.Kernel.Get<BroadcastCommunicatorFactoryInterface>();
                    return fact.GetCommunicatorForEndpoint(endpoint)
                        .GetScheduler();
                })
                .WithMetadata("BroadcastCommunicator", true);

            Trace.TraceInformation("Finished Binding ApplicationNinjectBinding");

        }

        #endregion
    }
}
