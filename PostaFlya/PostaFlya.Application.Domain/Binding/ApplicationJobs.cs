using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using PostaFlya.Application.Domain.SiteMap;
using Website.Application.Domain.TinyUrl;
using Website.Application.Schedule;
using Website.Infrastructure.Configuration;

namespace PostaFlya.Application.Domain.Binding
{
    public class ApplicationJobs : NinjectModule 
    {
        public override void Load()
        {
            //every day at 1 am
            Kernel.Get<SchedulerInterface>().Jobs.Add(new AbsoluteRepeatJob()
            {
                Id = "SiteMapBuilder",
                FriendlyId = "SiteMapBuilder",
                DayOfWeek = "*",
                HourOfDay = "1",                
                Minute = "0",
                JobStorage = new Dictionary<string, string>(),
                JobActionClass = typeof(SiteMapXmlGenJobAction)
            });
        }
    }
}
