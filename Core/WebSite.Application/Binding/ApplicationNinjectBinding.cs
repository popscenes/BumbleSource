﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using Website.Application.ApplicationCommunication;
using Website.Application.Command;
using Website.Application.Content;
using Website.Application.Email;
using Website.Application.Publish;
using Website.Application.Queue;
using Website.Application.Schedule;
using Website.Application.WebsiteInformation;
using Website.Infrastructure.Binding;
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

            Bind<QrCodeServiceInterface>()
                .To<ZXingQrCodeService>()
                .InSingletonScope();

            Bind<TimeServiceInterface>()
                .To<DefaultTimeService>()
                .InSingletonScope();

            Bind<SchedulerInterface>().ToMethod(context =>
                {
                    var ret = context.Kernel.Get<Scheduler>();
                    return ret;
                }).InSingletonScope();

            Bind<SendEmailServiceInterface>().To<QueuedSendEmailService>().InTransientScope();
            Bind<SendMailImplementationInterface>().To<SendGridSendMailImplementation>().InTransientScope();

            Trace.TraceInformation("Finished Binding ApplicationNinjectBinding");

        }

        #endregion
    }

}
