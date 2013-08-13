using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
            kernel.BindMessageAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());

            kernel.BindInfrastructureQueryHandlersForTypesFrom(
                c => c.InTransientScope(), Assembly.GetAssembly(typeof(PostaFlya.Domain.Browser.Browser)));

            kernel.BindEventHandlersFromCallingAssembly(c => c.InTransientScope());

            Trace.TraceInformation("Finished Binding CommandNinjectBinding");

        }
    }
}
