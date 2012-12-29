using Ninject;
using Ninject.Modules;
using Website.Application.Schedule;

namespace PostaFlya.Application.Domain.Binding
{
    public class JobsNinjectBinding : NinjectModule 
    {
        public override void Load()
        {
            var scheduler = Kernel.Get<SchedulerInterface>();
        }
    }
}