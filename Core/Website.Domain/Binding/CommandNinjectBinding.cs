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
            kernel.BindCommandAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());

            kernel.BindGenericQueryHandlersFromAssemblyForTypesFrom(Assembly.GetAssembly(typeof(FindByIdQuery)),
                Assembly.GetAssembly(typeof(Browser.Browser)), c => c.InTransientScope());

            Trace.TraceInformation("Finished Binding CommandNinjectBinding");

        }
    }
}
