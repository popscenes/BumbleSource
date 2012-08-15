using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Syntax;
using PostaFlya.Domain.Behaviour;
using WebSite.Infrastructure.Binding;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Domain.TaskJob.Binding
{
    public class TaskJobNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding TaskJobNinjectBinding");

            //behaviour factory bindings
            Kernel.Get<Dictionary<FlierBehaviour, Type>>(ctx => ctx.Has("flierbehaviour"))
                .Add(FlierBehaviour.TaskJob, typeof(TaskJobFlierBehaviourInterface));

            Bind<TaskJobFlierBehaviourInterface>()
                .To<TaskJobFlierBehaviour>();

            var kernel = Kernel as StandardKernel;
            kernel.BindCommandHandlersFromCallingAssembly(c => c.InTransientScope());

            Trace.TraceInformation("Finished Binding TaskJobNinjectBinding");

        }
    }
}
