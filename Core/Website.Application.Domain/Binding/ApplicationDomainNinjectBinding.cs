using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using Website.Application.Domain.Location;
using Website.Application.Domain.Payment;
using Website.Application.Domain.TinyUrl;
using Website.Application.Messaging;
using Website.Domain.Service;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Content;
using Website.Application.Domain.Content.Command;
using Website.Infrastructure.Messaging;

namespace Website.Application.Domain.Binding
{
    //this shouldn't be needed in specification projtect everything in application layer should be mocked
    public class ApplicationDomainNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationDomainNinjectBinding");

//            Kernel.Bind<BrowserInformationInterface<Website.Domain.Browser.Browser>>()
//                .To<BrowserInformation<Website.Domain.Browser.Browser>>()
//                .InRequestScope();

            Bind<RequestContentRetrieverFactoryInterface>().To<RequestContentRetrieverFactory>();

            Bind<UrlContentRetrieverFactoryInterface>().To<UrlContentRetrieverFactory>();

            Bind<ContentStorageServiceInterface>().To<ImageProcessContentStorageService>();

            Bind<EventPublishServiceInterface>().To<EventPublishService>();

            //this is for appication command handlers to use, 
            //need to consider putting context on this
            Bind<MessageBusInterface>()
                .To<InMemoryMessageBus>(); 

            //command handlers
            var kernel = Kernel as StandardKernel;
            BindCommandAndQueryHandlers(kernel);

            //publish services
            kernel.BindEventHandlersFromCallingAssembly(syntax => syntax.InTransientScope());

            Bind<TinyUrlServiceInterface>()
                .To<DefaultTinyUrlService>();

            Bind<GeoIpServiceInterface>()
                .To<MaxMindGeoIpService>().InThreadScope();

            Trace.TraceInformation("Finished Binding ApplicationDomainNinjectBinding");
        }

        public static void BindCommandAndQueryHandlers(StandardKernel kernel)
        {
            kernel.BindMessageAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());
        }

        #endregion
    }
}
