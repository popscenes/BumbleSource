﻿using System;
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
using Website.Infrastructure.Command;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;

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
            kernel.Bind(
                x => x.From(asm)
                         .IncludingNonePublicTypes()
                         .SelectAllClasses()
                         .Where(t => t.GetInterfaces().Any(i => i == typeof(HandleEventInterface))
                         )
                         .BindAllInterfaces()
                         .Configure(ninjectConfiguration));
        }

        public static void BindAllInterfacesFromAssemblyFor(this StandardKernel kernel, Assembly asm, Type type, ConfigurationAction ninjectConfiguration)
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

        public static void BindCommandAndQueryHandlersFromCallingAssembly(this StandardKernel kernel
                                                                  , ConfigurationAction ninjectConfiguration)
        {
            var asm = Assembly.GetCallingAssembly();
            Trace.TraceInformation("Binding command handlers from {0}", asm.FullName);
            //command handlers
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(CommandHandlerInterface<>), ninjectConfiguration);


            //query handlers
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(QueryHandlerInterface<,>), ninjectConfiguration);

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
