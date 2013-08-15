using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.FSharp.Collections;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using Ninject.Syntax;
using Website.FunctionalLib;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;
using Website.Infrastructure.Types;
using Website.Infrastructure.Util;
using Website.Infrastructure.Util.Extension;

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
                .WhenInjectedInto<DefaultMessageHandlerRepository>();
            Bind<MessageHandlerRespositoryInterface>().To<DefaultMessageHandlerRepository>().InSingletonScope();

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

        public static void BindMessageAndQueryHandlersFromCallingAssembly(this IKernel kernel
                                                                  , ConfigurationAction ninjectConfiguration)
        {
            var asm = Assembly.GetCallingAssembly();
            kernel.BindCommandAndQueryHandlersForAssembly(asm, ninjectConfiguration);
        }


        public static void BindCommandAndQueryHandlersForAssembly(this IKernel kernel, Assembly asm 
                                                          , ConfigurationAction ninjectConfiguration)
        {
            Trace.TraceInformation("Binding command handlers from {0}", asm.FullName);
            //command handlers
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(MessageHandlerInterface<>), ninjectConfiguration);


            //query handlers
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(QueryHandlerInterface<,>), ninjectConfiguration);
            Trace.TraceInformation("End Binding command handlers from {0}", asm.FullName);

        }



        public static void BindInfrastructureQueryHandlersForTypesFrom(
            this IKernel kernel, ConfigurationAction ninjectConfiguration
            ,params Assembly[] typeAssemblies)
        {

            var qh = typeof (FindByIdQueryHandler<>);
            var q = typeof (FindByIdQuery<>);
            var findByIds = TypeUtil.GetExpandedTypesUsing(q, typeAssemblies);
            var exp = TypeUtil.Expand(qh, findByIds.Select(a => a.GetGenericArguments().First()).ToList());
            foreach (var inst in exp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

            qh = typeof(FindByIdsQueryHandler<>);
            q = typeof(FindByIdsQuery<>);
            findByIds = TypeUtil.GetExpandedTypesUsing(q, typeAssemblies);
            exp = TypeUtil.Expand(qh, findByIds.Select(a => a.GetGenericArguments().First()).ToList());
            foreach (var inst in exp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

        }

        public static void BindToGenericInterface(this IKernel kernel, Type genericTyp, Type interfaceTyp)
        {
            var iface = genericTyp.GetInterfaces().FirstOrDefault(arg1 =>
                                               arg1.IsGenericType &&
                                               arg1.GetGenericTypeDefinition() == interfaceTyp);
            if (iface != null)
            {
                kernel.Bind(iface).To(genericTyp);
            }
        }

//        public static void BindGenericQueryHandlersFromAssemblyForTypesFrom2(this IKernel kernel
//            , Assembly asm                                                                       
//            , ConfigurationAction
//                ninjectConfiguration
//            ,
//            Func<Type, bool> typeSelector,
//            Func<Type, bool> querySelector,
//            params Assembly[] typeAssemblies)
//        {
//            Trace.TraceInformation("Binding generic query handler from {0} for {1}", asm.FullName, typeAssemblies.Aggregate("",(s, assembly) => s + " " + assembly.GetName()));
//
//            var genHandlers = asm.DefinedTypes
//                                 .Select(info => info.AsType())
//                                 .Where(
//                                     arg =>
//                                     arg.IsGenericType && !arg.IsInterface && arg.IsGenericTypeDefinition &&
//                                     typeof(QueryHandlerInterface).IsAssignableFrom(arg)
//                                     && (querySelector == null || querySelector(arg))).ToList();
//
//            if (typeSelector == null) typeSelector = arg => true;
//            var argTypes = typeAssemblies.SelectMany(assembly => assembly.DefinedTypes)  
//                        .Select(info => info.AsType())
//                        .Where(typeSelector).ToList();
//
//            argTypes = TypeUtil.ExpandGenericTypes(argTypes);
//
//            argTypes.AddRange(genHandlers);
//
//            var qhInt = typeof(QueryHandlerInterface<,>);
//
//            var all = TypeUtil.ExpandGenericTypes(argTypes);
//            all.ForEach(obj =>
//                {
//                    var iface = obj.GetInterfaces().FirstOrDefault(arg1 =>
//                                                                   arg1.IsGenericType &&
//                                                                   arg1.GetGenericTypeDefinition() == qhInt);
//                    if (iface != null)
//                    {
//                        kernel.Bind(iface).To(obj); 
//                    }
//                });
//
//        }


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
