using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Flier;
using Website.Infrastructure.Binding;
using Website.Application.Domain.Browser;

namespace PostaFlya.Application.Domain.Binding
{
    //this shouldn't be needed in specification projtect everything in application layer should be mocked
    public class ApplicationDomainNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationDomainNinjectBinding");

            //command handlers
            var kernel = Kernel as StandardKernel;
            kernel.BindCommandAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());

            kernel.BindEventHandlersFromCallingAssembly(c => c.InTransientScope());

            Kernel.Bind<PostaFlyaBrowserInformationInterface>()
                .To<PostaFlyaBrowserInformation>()
                .InRequestScope();

            Kernel.Bind<BrowserInformationInterface>()
                .To<PostaFlyaBrowserInformation>()
                .InRequestScope();

            Kernel.Bind<FlierWebAnalyticServiceInterface>()
                  .To<DefaultFlierWebAnalyticService>()
                  .InRequestScope();

            Trace.TraceInformation("Finished Binding ApplicationDomainNinjectBinding");
        }

        #endregion
    }
}
