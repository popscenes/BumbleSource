using System;
using Ninject;
using Ninject.Modules;
using PostaFlya.Areas.TaskJob.Models.Factory;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Models.Factory;

namespace PostaFlya.Areas.TaskJob.Binding
{
    public class TaskJobBehaviourWebNinjectBinding :  NinjectModule
    {
        public override void Load()
        {
            Kernel.Get<FlierBehaviourViewModelFactoryRegistryInterface>()
                .RegisterViewModelFactory(typeof(TaskJobFlierBehaviourInterface), 
                    new TaskJobBehaviourFlierBehaviourViewModelFactory());
        }
    }
}