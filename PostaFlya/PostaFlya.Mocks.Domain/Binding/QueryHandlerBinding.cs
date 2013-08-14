using System.Reflection;
using Ninject;
using Ninject.Modules;
using Website.Domain.Comments;
using Website.Infrastructure.Binding;
using Website.Mocks.Domain.Binding;

namespace PostaFlya.Mocks.Domain.Binding
{
    public class QueryHandlerBinding : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as StandardKernel;



//            kernel.BindGenericQueryHandlersFromAssemblyForTypesFrom(
//                Assembly.GetAssembly(typeof(Website.Mocks.Domain.Binding.QueryHandlerBinding))
//                , syntax => syntax.InTransientScope(), null, null
//                , Assembly.GetAssembly(typeof(PostaFlya.Domain.Flier.Flier)));

            kernel.BindMockDomainQueryHandlersForTypesFrom(syntax => syntax.InTransientScope()
                    , Assembly.GetAssembly(typeof(Comment)) 
                    , Assembly.GetAssembly(typeof (PostaFlya.Domain.Flier.Flier)));
            kernel.BindMessageAndQueryHandlersFromCallingAssembly(syntax => syntax.InTransientScope());

        }
    }
}
