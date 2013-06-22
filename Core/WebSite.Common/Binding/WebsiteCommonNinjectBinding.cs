using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Query;

namespace Website.Common.Binding
{
    public class WebsiteCommonNinjectBinding : NinjectModule 
    {
        
        public override void Load()
        {
            Trace.TraceInformation("Binding WebsiteCommonNinjectBinding");

            var kernel = Kernel as StandardKernel;
            kernel.BindCommandAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());
        }
    }

    public static class WebsiteCommonNinjectExtensions
    {
        public static void BindViewModelMappersFromCallingAssembly(this IKernel kernel)
        {
            Trace.TraceInformation("Binding BindViewModelMappersFromCallingAssembly");
            var asm = Assembly.GetCallingAssembly();
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(ViewModelMapperInterface<,>), c => c.InThreadScope());
        }
    }
}
