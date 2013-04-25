using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Syntax;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Binding
{
    public class CommandNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding CommandNinjectBinding");
            
            //command handlers
            var kernel = Kernel as StandardKernel;
            kernel.BindCommandAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());

            Trace.TraceInformation("Finished Binding CommandNinjectBinding");

        }
    }
}
