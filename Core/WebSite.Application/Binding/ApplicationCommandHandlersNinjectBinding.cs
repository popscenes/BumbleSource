using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using Website.Infrastructure.Binding;

namespace Website.Application.Binding
{
    public class ApplicationCommandHandlersNinjectBinding :  NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationCommandHandlersNinjectBinding");
            
            var kernel = Kernel as StandardKernel;
            kernel.BindCommandHandlersFromCallingAssembly(c => c.InTransientScope());

            Trace.TraceInformation("Finished Binding ApplicationCommandHandlersNinjectBinding");
        }
    }
}
