using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.FunctionalLib;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Query;
using Website.Infrastructure.Types;
using Website.Infrastructure.Util.Extension;

namespace Website.Common.Binding
{
    public class WebsiteCommonNinjectBinding : NinjectModule 
    {
        
        public override void Load()
        {
            Trace.TraceInformation("Binding WebsiteCommonNinjectBinding");

            var kernel = Kernel as StandardKernel;
            
            kernel.BindCommandAndQueryHandlersForAssembly(Assembly.GetAssembly(typeof(WebsiteCommonNinjectBinding)), 
                c => c.InTransientScope());

            Trace.TraceInformation("End Binding WebsiteCommonNinjectBinding");

        }
    }

    public static class WebsiteCommonNinjectExtensions
    {
        public static void BindViewModelMappersFromCallingAssembly(this IKernel kernel)
        {
            
            var asm = Assembly.GetCallingAssembly();
            Trace.TraceInformation("Binding BindViewModelMappersFromCallingAssembly " + asm.FullName);
            kernel.BindAllInterfacesFromAssemblyFor(asm, typeof(ViewModelMapperInterface<,>), c => c.InThreadScope());
        }


        public static void BindWebsiteCommonQueryHandlersForTypesFrom(
            this IKernel kernel, ConfigurationAction ninjectConfiguration
            , params Assembly[] typeAssemblies)
        {
            
            var qh = typeof(AsViewModelQueryHandler<,,>);
            var q = typeof (QueryInterface<>);
            var queries = TypeUtil.GetExpandedImplementorsUsing(q, typeAssemblies).ToFSharpList();
            var models = TypeUtil.GetAllSubTypesFrom(typeof (IsModelInterface), typeAssemblies).ToFSharpList();
            var combo = new List<FSharpList<Type>>() {queries, models}.ToFSharpList();

            var func = new Lists();
            var all = func.Cartesian<FSharpList<Type>, Type>(combo);
            foreach (var a in all)
            {
                var query = a.Head;
                var model = a.Tail.Head;
                var qtype = query.GetInterfaces().First(arg => arg.GetGenericTypeDefinition() == q);
                var inst = qh.MakeGenericType(query, qtype.GetGenericArguments().First(), model);
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }
            
            
            qh = typeof(AsViewModelsQueryHandler<,,>);
            q = typeof(QueryInterface<>);
            queries = TypeUtil.GetExpandedImplementorsUsing(q, typeAssemblies).ToFSharpList();
            models = TypeUtil.GetAllSubTypesFrom(typeof(IsModelInterface), typeAssemblies).ToFSharpList();
            combo = new List<FSharpList<Type>>() { queries, models }.ToFSharpList();

            all = func.Cartesian<FSharpList<Type>, Type>(combo);
            foreach (var a in all)
            {
                var query = a.Head;
                var model = a.Tail.Head;
                var qtype = query.GetInterfaces().First(arg => arg.GetGenericTypeDefinition() == q);
                var inst = qh.MakeGenericType(query, qtype.GetGenericArguments().First(), model);
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

        }
        
    }
}
