using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using Website.Infrastructure.Binding;

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

            Trace.TraceInformation("Finished Binding CommandNinjectBinding");

        }
    }
}
