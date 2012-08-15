using System.Diagnostics;
using System.Linq;
using Ninject;
using Ninject.Modules;
using WebSite.Application.Command;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Content;
using PostaFlya.Application.Domain.Content.Command;
using PostaFlya.Domain.Service;
using WebSite.Infrastructure.Binding;
using WebSite.Infrastructure.Command;
using Ninject.Extensions.Conventions;
using PostaFlya.Application.Domain.ExternalSource;

namespace PostaFlya.Application.Domain.Binding
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

            Bind<FlierImportServiceInterface>().To<FlierImportService>();

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
