﻿using System.Diagnostics;
using System.Linq;
using Ninject;
using Ninject.Modules;
using PostaFlya.Application.Domain.ExternalSource;
using Website.Application.Binding;
using Website.Application.Command;
using PostaFlya.Domain.Service;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Ninject.Extensions.Conventions;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Content;
using Website.Application.Domain.Content.Command;
using Website.Domain.Service;

namespace PostaFlya.Application.Domain.Binding
{
    //this shouldn't be needed in specification projtect everything in application layer should be mocked
    public class ApplicationDomainNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationDomainNinjectBinding");

            //Kernel.Bind<BrowserInformationInterface>().To<BrowserInformation>();

            //Bind<RequestContentRetrieverFactoryInterface>().To<RequestContentRetrieverFactory>();

            //Bind<UrlContentRetrieverFactoryInterface>().To<UrlContentRetrieverFactory>();

            //Bind<ContentStorageServiceInterface>().To<ImageProcessContentStorageService>();

            Bind<FlierImportServiceInterface>().To<FlierImportService>();

            //this is for appication command handlers to use, 
            //need to consider putting context on this
            //Bind<CommandBusInterface>()
            //    .To<DefaultCommandBus>(); 

            //command handlers
            var kernel = Kernel as StandardKernel;
            kernel.BindCommandHandlersFromCallingAssembly(c => c.InTransientScope());

            kernel.BindPublishServicesFromCallingAssembly(c => c.InTransientScope());

            Trace.TraceInformation("Finished Binding ApplicationDomainNinjectBinding");
        }

        #endregion
    }
}
