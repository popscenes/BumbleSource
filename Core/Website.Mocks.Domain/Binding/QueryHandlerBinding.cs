using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Query;
using Website.Infrastructure.Types;
using Website.Mocks.Domain.DomainQuery;
using Website.Mocks.Domain.DomainQuery.Browser;

namespace Website.Mocks.Domain.Binding
{
    public class QueryHandlerBinding : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as StandardKernel;


            kernel.BindMessageAndQueryHandlersFromCallingAssembly(syntax => syntax.InTransientScope());
        }
    }

    public static class MockDomainBindingExtensions
    {
        public static void BindMockDomainQueryHandlersForTypesFrom(
            this IKernel kernel, ConfigurationAction ninjectConfiguration
            , params Assembly[] typeAssemblies)
        {

            var qh = typeof(TestFindByFriendlyIdQueryHandler<>);
            var qhExp = TypeUtil.GetExpandedTypesUsing(qh, typeAssemblies);
            foreach (var inst in qhExp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

            qh = typeof(TestGetAggregateByBrowserIdQueryHandler<>);
            qhExp = TypeUtil.GetExpandedTypesUsing(qh, typeAssemblies);
            foreach (var inst in qhExp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

            qh = typeof(TestFindBrowserByIdentityProviderQueryHandler<>);
            qhExp = TypeUtil.GetExpandedTypesUsing(qh, typeAssemblies);
            foreach (var inst in qhExp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

            qh = typeof(TestGetByBrowserIdQueryHandler<>);
            qhExp = TypeUtil.GetExpandedTypesUsing(qh, typeAssemblies);
            foreach (var inst in qhExp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }
            
        }
    }
}
