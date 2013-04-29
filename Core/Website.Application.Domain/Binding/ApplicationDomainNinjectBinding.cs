using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using Website.Application.Domain.Payment;
using Website.Application.Domain.Publish;
using Website.Application.Domain.TinyUrl;
using Website.Domain.Service;
using Website.Domain.TinyUrl;
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

            Kernel.Bind<BrowserInformationInterface>().To<BrowserInformation>().InRequestScope();

            Bind<RequestContentRetrieverFactoryInterface>().To<RequestContentRetrieverFactory>();

            Bind<UrlContentRetrieverFactoryInterface>().To<UrlContentRetrieverFactory>();

            Bind<ContentStorageServiceInterface>().To<ImageProcessContentStorageService>();

            Bind<DomainEventPublishServiceInterface>().To<DomainEventPublishService>();

            //this is for appication command handlers to use, 
            //need to consider putting context on this
            Bind<CommandBusInterface>()
                .To<DefaultCommandBus>(); 

            //command handlers
            var kernel = Kernel as StandardKernel;
            BindCommandAndQueryHandlers(kernel);

            //publish services
            kernel.BindEventHandlersFromCallingAssembly(syntax => syntax.InTransientScope());

            Bind<TinyUrlServiceInterface>()
                .To<DefaultTinyUrlService>();

            Trace.TraceInformation("Finished Binding ApplicationDomainNinjectBinding");
        }

        public static void BindCommandAndQueryHandlers(StandardKernel kernel)
        {
            kernel.BindCommandAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());
        }

        #endregion
    }
}
