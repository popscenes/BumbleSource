using System.Diagnostics;
using System.Linq;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.ExternalSource;
using PostaFlya.Application.Domain.Flier;
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

            //command handlers
            var kernel = Kernel as StandardKernel;
            kernel.BindCommandAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());

            kernel.BindEventHandlersFromCallingAssembly(c => c.InTransientScope());

            Kernel.Bind<PostaFlyaBrowserInformationInterface>()
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
