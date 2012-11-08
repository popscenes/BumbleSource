﻿using System;
using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using Website.Application.ApplicationCommunication;
using Website.Application.Command;
using Website.Application.Publish;
using Website.Application.WebsiteInformation;
using Website.Infrastructure.Publish;

namespace Website.Application.Binding
{
    public class ApplicationNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationNinjectBinding");

            Kernel.Bind<WebsiteInfoServiceInterface>().To<CachedWebsiteInfoService>();

            //broadcast communicator
            Kernel.Bind<ApplicationBroadcastCommunicatorFactoryInterface>()
                .To<DefaultApplicationBroadcastCommunicatorFactory>()
                .InSingletonScope();

            Bind<ApplicationBroadcastCommunicatorInterface>()
                .ToMethod(ctx =>
                {
                    var idFunc = ctx.Kernel.Get<Func<string>>(metadata => metadata.Has("BroadcastCommunicator"));
                    var endpoint = idFunc();
                    var fact = ctx.Kernel.Get<ApplicationBroadcastCommunicatorFactoryInterface>();
                    return fact.GetCommunicatorForEndpoint(endpoint);
                })
                .WithMetadata("BroadcastCommunicator", true);

            Bind<QueuedCommandProcessor>()
                .ToMethod(ctx =>
                {
                    var idFunc = ctx.Kernel.Get<Func<string>>(metadata => metadata.Has("BroadcastCommunicator"));
                    var endpoint = idFunc();
                    var fact = ctx.Kernel.Get<ApplicationBroadcastCommunicatorFactoryInterface>();
                    return fact.GetCommunicatorForEndpoint(endpoint)
                        .GetScheduler();
                })
                .WithMetadata("BroadcastCommunicator", true);

            Bind<BroadcastServiceInterface>()
                .To<DefaultBroadcastService>();

            Trace.TraceInformation("Finished Binding ApplicationNinjectBinding");

        }

        #endregion
    }

}
