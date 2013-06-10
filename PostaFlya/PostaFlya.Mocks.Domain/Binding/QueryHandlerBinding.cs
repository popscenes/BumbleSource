using System.Reflection;
using Ninject;
using Ninject.Modules;
using Website.Infrastructure.Binding;

namespace PostaFlya.Mocks.Domain.Binding
{
    public class QueryHandlerBinding : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as StandardKernel;

            kernel.BindGenericQueryHandlersFromCallingAssemblyForTypesFrom(Assembly.GetAssembly(typeof(PostaFlya.Domain.Flier.Flier))
                , syntax => syntax.InTransientScope());

            kernel.BindGenericQueryHandlersFromAssemblyForTypesFrom(
                Assembly.GetAssembly(typeof(Website.Mocks.Domain.Binding.QueryHandlerBinding)),
                Assembly.GetAssembly(typeof(PostaFlya.Domain.Flier.Flier))
                , syntax => syntax.InTransientScope());
        }
    }
}
