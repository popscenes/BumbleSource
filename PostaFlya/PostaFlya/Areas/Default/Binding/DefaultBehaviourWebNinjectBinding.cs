using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;
using Ninject.Modules;
using PostaFlya.Areas.Default.Models.Factory;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Models.Factory;

namespace PostaFlya.Areas.Default.Binding
{
    public class DefaultBehaviourWebNinjectBinding :  NinjectModule
    {
        public override void Load()
        {
            Kernel.Get<FlierBehaviourViewModelFactoryRegistryInterface>()
                .RegisterViewModelFactory(typeof(FlierBehaviourInterface), new DefaultBehaviourFlierBehaviourViewModelFactory());
        }
    }
}