using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using Website.Application.Schedule;
using Website.Application.TinyUrl;
using Website.Infrastructure.Configuration;

namespace Website.Application.Binding
{
    public class ApplicationJobs : NinjectModule
    {
        public override void Load()
        {
            Kernel.Get<SchedulerInterface>().Jobs.Add(new RepeatJob()
            {
                Id = "TinyUrlGenerator",
                FriendlyId = "Tiny Url Generator",
                RepeatSeconds = 60,
                JobStorage = TinyUrlGenerationJobAction.GetDefaults(Kernel.Get<ConfigurationServiceInterface>()),
                JobActionClass = typeof(TinyUrlGenerationJobAction)
            });
        }
    }
}
