using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Trace.TraceInformation("Binding ApplicationJobs");
            //every day at 1 am
            var sch = Kernel.TryGet<SchedulerInterface>();
            if (sch != null)
            {
                sch.Jobs.Add(new AbsoluteRepeatJob()
                    {
                        Id = "SiteMapBuilder",
                        FriendlyId = "SiteMapBuilder",
                        DayOfWeek = "*",
                        HourOfDay = "1",
                        Minute = "0",
                        JobStorage = new Dictionary<string, string>(),
                        JobActionClass = typeof (SiteMapXmlGenJobAction),
                        TimeOut = TimeSpan.FromDays(1)
                    });
            }

            Trace.TraceInformation("End Binding ApplicationJobs");
        }
    }
}
