using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using PostaFlya.Application.Domain.ExternalSource;
using PostaFlya.Application.Domain.Flier;

namespace PostaFlya.Application.Domain.Binding
{
    public class ApplicationDomainServicesNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationDomainServicesNinjectBinding");

            Bind<FlierImportServiceInterface>().To<FlierImportService>();
            Bind<FlierPrintImageServiceInterface>()
                .To<DefaultFlierPrintImageService>()
                .InTransientScope();

            Trace.TraceInformation("Binding ApplicationDomainServicesNinjectBinding");
        }
    }
}
