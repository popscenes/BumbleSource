using System;
using System.Collections.Generic;
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
            var kernel = Kernel as StandardKernel;
            kernel.BindCommandAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());
        }
    }

    public static class WebsiteCommonNinjectExtensions
    {
        public static void BindViewModelMappersFromCallingAssembly(this StandardKernel kernel)
        {
            var asm = Assembly.GetCallingAssembly();
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(ViewModelMapperInterface<,>), c => c.InThreadScope());
        }
    }
}
