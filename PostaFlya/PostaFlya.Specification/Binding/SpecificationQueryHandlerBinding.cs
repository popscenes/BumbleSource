using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using Website.Infrastructure.Binding;

namespace PostaFlya.Specification.Binding
{
    public class SpecificationQueryHandlerBinding : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as StandardKernel;

            kernel.BindGenericQueryHandlersFromCallingAssemblyForTypesFrom(Assembly.GetAssembly(typeof(PostaFlya.Domain.Flier.Flier))
                , syntax => syntax.InTransientScope());

            kernel.BindGenericQueryHandlersFromCallingAssemblyForTypesFrom(Assembly.GetAssembly(typeof(Website.Domain.Claims.Claim))
                , syntax => syntax.InTransientScope());
        }
    }
}
