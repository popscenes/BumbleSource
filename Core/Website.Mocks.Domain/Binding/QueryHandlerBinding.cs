using System.Reflection;
using Ninject;
using Ninject.Modules;
using Website.Infrastructure.Binding;

namespace Website.Mocks.Domain.Binding
{
    public class QueryHandlerBinding : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as StandardKernel;

            kernel.BindGenericQueryHandlersFromCallingAssemblyForTypesFrom(Assembly.GetAssembly(typeof(Website.Domain.Claims.Claim))
                , syntax => syntax.InTransientScope());
            kernel.BindCommandAndQueryHandlersFromCallingAssembly(syntax => syntax.InTransientScope());
        }
    }
}
