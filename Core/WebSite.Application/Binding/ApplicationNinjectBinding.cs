using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using Website.Application.ApplicationCommunication;
using Website.Application.Command;
using Website.Application.Content;
using Website.Application.Publish;
using Website.Application.Queue;
using Website.Application.Schedule;
using Website.Application.WebsiteInformation;
using Website.Infrastructure.Publish;

namespace Website.Application.Binding
{
    public class ApplicationNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationNinjectBinding");

            Kernel.Bind<WebsiteInfoServiceInterface>().To<CachedWebsiteInfoService>();

            //broadcast communicator
            Kernel.Bind<ApplicationBroadcastCommunicatorFactoryInterface>()
                .To<DefaultApplicationBroadcastCommunicatorFactory>()
                .InSingletonScope();

            Bind<ApplicationBroadcastCommunicatorInterface>()
                .ToMethod(ctx =>
                {
                    var idFunc = ctx.Kernel.Get<Func<string>>(metadata => metadata.Has("BroadcastCommunicator"));
                    var endpoint = idFunc();
                    var fact = ctx.Kernel.Get<ApplicationBroadcastCommunicatorFactoryInterface>();
                    return fact.GetCommunicatorForEndpoint(endpoint);
                })
                .WithMetadata("BroadcastCommunicator", true);

            Bind<QueuedCommandProcessor>()
                .ToMethod(ctx =>
                {
                    var idFunc = ctx.Kernel.Get<Func<string>>(metadata => metadata.Has("BroadcastCommunicator"));
                    var endpoint = idFunc();
                    var fact = ctx.Kernel.Get<ApplicationBroadcastCommunicatorFactoryInterface>();
                    return fact.GetCommunicatorForEndpoint(endpoint)
                        .GetScheduler();
                })
                .WithMetadata("BroadcastCommunicator", true);

            Bind<BroadcastServiceInterface>()
                .To<DefaultBroadcastService>();

            Bind<QrCodeServiceInterface>()
                .To<ZXingQrCodeService>()
                .InSingletonScope();

            Bind<QueueInterface>()
                .ToMethod(ctx => 
                    ctx.Kernel.Get<QueueFactoryInterface>().GetQueue("tinyurls"))
                .WhenTargetHas<TinyUrlQueue>()
                .InThreadScope();

            Bind<TimeServiceInterface>()
                .To<DefaultTimeService>()
                .InSingletonScope();

            Bind<SchedulerInterface>().ToMethod(context =>
                {
                    var ret = context.Kernel.Get<Scheduler>();
                    ret.RunInterval = 60000;
                    return ret;
                }).InSingletonScope();
            
            Kernel.Get<SchedulerInterface>().Jobs.Add(new RepeatJob()
                {
                    Id = "TinyUrlGenerator",
                    FriendlyId = "Tiny Url Generator",
                    RepeatSeconds = 60,
                    JobStorage = TinyUrlJobAction.GetDefaults(),
                    JobActionCommandClass = typeof(TinyUrlJobAction)
                });

            Trace.TraceInformation("Finished Binding ApplicationNinjectBinding");

        }

        #endregion
    }

    public class TinyUrlJobAction : JobActionInterface
    {
        private const string UrlBase = "urlbase";
        private const string StartPath = "startpath";

        public static Dictionary<string, string> GetDefaults()
        {
            return new Dictionary<string, string>() { { UrlBase, "http://pfly.in/" }, { StartPath, "" } };
        }
        private readonly QueueInterface _urlQueue;

        public TinyUrlJobAction([TinyUrlQueue]QueueInterface urlQueue)
        {
            _urlQueue = urlQueue;
        }

        public void Run(JobBase job)
        {
            if (!_urlQueue.ApproximateMessageCount.HasValue || _urlQueue.ApproximateMessageCount.Value > 5000)
                return;

            if (job.JobStorage == null || !job.JobStorage.ContainsKey(UrlBase))
                job.JobStorage = GetDefaults();

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
