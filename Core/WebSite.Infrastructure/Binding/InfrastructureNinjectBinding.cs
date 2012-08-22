using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using Ninject.Syntax;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;

namespace WebSite.Infrastructure.Binding
{
    public class InfrastructureNinjectBinding : NinjectModule
    {

        private static IResolutionRoot _resolver;
        //binds all generic commandhandler interfaces to their closed handler types.
        public override void Load()
        {
            Trace.TraceInformation("Binding InfrastructureNinjectBinding");

            //command handler repository
            _resolver = Kernel;
            Bind<IResolutionRoot>()
                .ToMethod(ctx => _resolver)
                .WhenInjectedInto<DefaultCommandHandlerRepository>();
            Bind<CommandHandlerRespositoryInterface>().To<DefaultCommandHandlerRepository>().InSingletonScope();

            Bind<UnitOfWorkFactoryInterface>().To<UnitOfWorkFactory>();

            Trace.TraceInformation("Finished Binding InfrastructureNinjectBinding");

        }
    }

    public static class InfrastructureNinjectExtensions
    {
        public static void BindCommandHandlersFromCallingAssembly(this StandardKernel kernel
                                                                  , ConfigurationAction ninjectConfiguration)
        {
            var asm = Assembly.GetCallingAssembly();
            Trace.TraceInformation("Binding command handlers from {0}", asm.FullName);
            //command handlers
            kernel.Bind(
                x => x.From(asm)
                         .IncludingNonePublicTypes()
                         .SelectAllClasses()
                         .Where(t => t.GetInterfaces().Any(i =>
                                                           i.IsGenericType &&
                                                           i.GetGenericTypeDefinition() ==
                                                           typeof(CommandHandlerInterface<>))
                         )
                         .BindAllInterfaces()
                         .Configure(ninjectConfiguration));
        }

        public static void BindRepositoriesFromCallingAssembly(this StandardKernel kernel
            , ConfigurationAction ninjectConfiguration
            , Type[] excludeInterfaces = null)
        {
            var asm = Assembly.GetCallingAssembly();
            Trace.TraceInformation("Binding Repositories from {0}", asm.FullName);
//            //repos
//            kernel.Bind(
//                x => x.From(asm)
//                         .IncludingNonePublicTypes()
//                         .SelectAllClasses()
//                         .Where(t => t.GetInterfaces().Any(i => i == typeof(RepositoryInterface)))
//                         .BindUsingRegex("Repository")
//                         .Configure(ninjectConfiguration));
//
//            //bind the query services (sparating so we can bind the QS's to caching versions)
//            kernel.Bind(
//                x => x.From(asm)
//                     .IncludingNonePublicTypes()
//                     .SelectAllClasses()
//                     .Where(t => t.GetInterfaces().Any(i => i == typeof(QueryServiceInterface)))
//                     .BindUsingRegex("QueryService")
//                     .Configure(ninjectConfiguration));


            kernel.Bind(
            x => x.From(asm)
                .IncludingNonePublicTypes()
                .SelectAllClasses()
                .Where(t => t.GetInterfaces().Any(i => i == typeof(RepositoryInterface) || i == typeof(QueryServiceInterface)))
                //.BindAllInterfaces()
                .BindSelection((type, types) => 
                    types.Where(t => 
                        excludeInterfaces == null || !excludeInterfaces.Contains(t) 
                ))
                .Configure(ninjectConfiguration));

        }
    }
}
