using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using Ninject.Syntax;
using Website.Infrastructure.Command;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util;

namespace Website.Infrastructure.Binding
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

            Bind<QueryChannelInterface>().To<DefaultQueryChannel>().InThreadScope();

            Trace.TraceInformation("Finished Binding InfrastructureNinjectBinding");

        }
    }

    public static class InfrastructureNinjectExtensions
    {

        public static void BindEventHandlersFromCallingAssembly(this StandardKernel kernel
                                                          , ConfigurationAction ninjectConfiguration)
        {
            var asm = Assembly.GetCallingAssembly();
            Trace.TraceInformation("Binding Subscribers from {0}", asm.FullName);
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(HandleEventInterface), ninjectConfiguration);
        }

        public static void BindAllInterfacesFromAssemblyFor(this IKernel kernel, Assembly asm, Type type, ConfigurationAction ninjectConfiguration)
        {
            kernel.Bind(
                x => x.From(asm)
                    .IncludingNonePublicTypes()
                    .SelectAllClasses()
                    .Where(t =>
                        t.GetInterfaces().Any(i =>
                                (i.IsGenericType &&
                                i.GetGenericTypeDefinition() ==
                                type) || i == type)
                    )
                    .BindAllInterfaces()
                    .Configure(ninjectConfiguration));
        }

        public static void BindCommandAndQueryHandlersFromCallingAssembly(this IKernel kernel
                                                                  , ConfigurationAction ninjectConfiguration)
        {
            var asm = Assembly.GetCallingAssembly();
            Trace.TraceInformation("Binding command handlers from {0}", asm.FullName);
            //command handlers
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(CommandHandlerInterface<>), ninjectConfiguration);


            //query handlers
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(QueryHandlerInterface<,>), ninjectConfiguration);

        }

        public static void BindGenericQueryHandlersFromCallingAssemblyForTypesFrom(this IKernel kernel
            , Assembly typeAssembly, Func<Type, bool> typeSelector 
            , ConfigurationAction ninjectConfiguration)
        {
            var asm = Assembly.GetCallingAssembly();
            Trace.TraceInformation("Binding generic query handler from {0} for {1}", asm.FullName, typeAssembly.FullName);
            
            var genHandlers = asm.DefinedTypes
                                 .Select(info => info.AsType())
                                 .Where(
                                     arg =>
                                     arg.IsGenericType &&
                                     typeof(QueryHandlerInterface).IsAssignableFrom(arg) ).ToList();

            var argTypes = typeAssembly.DefinedTypes
                        .Select(info => info.AsType())
                        .Where(typeSelector).ToList();

            var qhInt = typeof(QueryHandlerInterface<,>);
            foreach (var handler in genHandlers)
            {
                var arg = handler.GetGenericArguments().First();
                foreach (var argType in argTypes)
                {
                    var inst = handler.MakeGenericType(argType);
                    var iface = inst.GetInterfaces().FirstOrDefault(arg1 => 
                                                            arg1.IsGenericType && arg1.GetGenericTypeDefinition() == qhInt);
                    kernel.Bind(iface).To(inst);
                }
            }
        }

        public static void BindRepositoriesFromCallingAssembly(this StandardKernel kernel
            , ConfigurationAction ninjectConfiguration
            , Type[] excludeInterfaces = null)
        {
            var asm = Assembly.GetCallingAssembly();
            Trace.TraceInformation("Binding Repositories from {0}", asm.FullName);

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
