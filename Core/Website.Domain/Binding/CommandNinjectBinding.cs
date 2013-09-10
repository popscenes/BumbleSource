using System.Diagnostics;
using System.Reflection;
using Ninject;
using Ninject.Modules;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Query;

namespace Website.Domain.Binding
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
                c => c.InTransientScope(), Assembly.GetAssembly(typeof(Domain.Browser.Browser)));

            Trace.TraceInformation("Finished Binding CommandNinjectBinding");

        }
    }
}
