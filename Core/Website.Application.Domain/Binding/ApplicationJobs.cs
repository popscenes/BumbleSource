using Ninject;
using Ninject.Modules;
using Website.Application.Domain.TinyUrl;
using Website.Application.Schedule;
using Website.Infrastructure.Configuration;

namespace Website.Application.Domain.Binding
{
    public class ApplicationJobs : NinjectModule
    {
        public override void Load()
        {
            Kernel.Get<SchedulerInterface>().Jobs.Add(new RepeatJob()
            {
                Id = "TinyUrlGenerator",
                FriendlyId = "Tiny Url Generator",
                RepeatSeconds = 120,
                JobStorage = TinyUrlGenerationJobAction.GetDefaults(Kernel.Get<ConfigurationServiceInterface>()),
                JobActionClass = typeof(TinyUrlGenerationJobAction)
            });
        }
    }
}
