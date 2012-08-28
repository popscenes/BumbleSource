using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using Website.Application.Domain.Claims;
using Website.Domain.Service;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Content;
using Website.Application.Domain.Content.Command;

namespace Website.Application.Domain.Binding
{
    //this shouldn't be needed in specification projtect everything in application layer should be mocked
    public class ApplicationDomainNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationDomainNinjectBinding");

            Kernel.Bind<BrowserInformationInterface>().To<BrowserInformation>();

            Bind<RequestContentRetrieverFactoryInterface>().To<RequestContentRetrieverFactory>();

            Bind<UrlContentRetrieverFactoryInterface>().To<UrlContentRetrieverFactory>();

            Bind<ContentStorageServiceInterface>().To<ImageProcessContentStorageService>();

            Bind<ClaimPublicationServiceInterface>().To<ClaimPublicationService>();


            //this is for appication command handlers to use, 
            //need to consider putting context on this
            Bind<CommandBusInterface>()
                .To<DefaultCommandBus>(); 

            //command handlers
            var kernel = Kernel as StandardKernel;
            kernel.BindCommandHandlersFromCallingAssembly(c => c.InTransientScope());

            Trace.TraceInformation("Finished Binding ApplicationDomainNinjectBinding");
        }

        #endregion
    }
}
