using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using Website.Application.Queue;
using Website.Application.Schedule;
using Website.Infrastructure.Configuration;

namespace Website.Application.Binding
{
    public class ApplicationJobs : NinjectModule
    {
        public override void Load()
        {
//            Kernel.Get<SchedulerInterface>().Jobs.Add(new RepeatJob()
//            {
//                Id = "TinyUrlGenerator",
//                FriendlyId = "Tiny Url Generator",
//                RepeatSeconds = 60,
//                JobStorage = TinyUrlJobAction.GetDefaults(Kernel.Get<ConfigurationServiceInterface>()),
//                JobActionClass = typeof(TinyUrlJobAction)
//            });
        }
    }

    public class TinyUrlJobAction : JobActionInterface
    {
        private const string UrlBase = "urlbase";
        private const string StartPath = "startpath";

        public static Dictionary<string, string> GetDefaults(ConfigurationServiceInterface configurationService)
        {
            var url = configurationService != null ? configurationService.GetSetting("TinyUrlBase") : null;
            if (string.IsNullOrEmpty(url))
                url = "http://pfly.in/";
            return new Dictionary<string, string>() { { UrlBase, url }, { StartPath, "" } };
        }
        private readonly QueueInterface _urlQueue;
        private readonly ConfigurationServiceInterface _configurationService;

        public TinyUrlJobAction([TinyUrlQueue]QueueInterface urlQueue
            , ConfigurationServiceInterface configurationService = null)
        {
            _urlQueue = urlQueue;
            _configurationService = configurationService;
        }

        public void Run(JobBase job)
        {
            if (!_urlQueue.ApproximateMessageCount.HasValue || _urlQueue.ApproximateMessageCount.Value > 5000)
                return;

            if (job.JobStorage == null || !job.JobStorage.ContainsKey(UrlBase))
                job.JobStorage = GetDefaults(_configurationService);

            for (var i = 0; i < 10000; i++)
            {
                AddNewUrlToQueue(job);
            }
        }

        private void AddNewUrlToQueue(JobBase job)
        {

            var start = job.JobStorage[StartPath];
            var next = Increment(start);

            _urlQueue.AddMessage(
                new QueueMessage(System.Text.Encoding.ASCII.GetBytes(job.JobStorage[UrlBase] + next))
            );

            job.JobStorage[StartPath] = next;

        }

        private static string Increment(string start)
        {
            if (string.IsNullOrEmpty(start))
                return "0";

            var last = start[start.Length - 1];
            last++;
            if (last == 'z' + 1)
                return start + '0';
            if (last == '0' + 10)
                return start.Remove(start.Length - 1) + 'a';
            return start.Remove(start.Length - 1) + last;

        }
    }
}
