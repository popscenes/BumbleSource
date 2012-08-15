using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using Ninject.Modules;
using WebSite.Application.Command;

namespace PostaFlya.CommandWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly QueuedCommandWorker _commandWorker;

        public WorkerRole()
        {
            _commandWorker = new QueuedCommandWorker();
            Trace.WriteLine("WorkerRole created", "Information");        
        }

        public override void Run()
        {
            Trace.WriteLine("PostaFlya.CommandWorker Run start", "Information");
            _commandWorker.Run();       
            base.Run();
            Trace.WriteLine("PostaFlya.CommandWorker Run end", "Information");
        }

        public override bool OnStart()
        {
            Trace.WriteLine("PostaFlya.CommandWorker OnStart start", "Information");

            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            _commandWorker.Init();
            var ret =  base.OnStart();
            Trace.WriteLine("PostaFlya.CommandWorker OnStart end", "Information");
            return ret;
        }

        public override void OnStop()
        {
            Trace.WriteLine("PostaFlya.CommandWorker OnStop start", "Information");
            _commandWorker.Stop();
            base.OnStop();
            Trace.WriteLine("PostaFlya.CommandWorker OnStop end", "Information");
        }
    }

//    public static class AllBindings
//    {
//        public static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
//                  {
//                      new PostaFlya.Domain.Binding.DefaultServicesNinjectBinding(),
//                      new WebSite.Infrastructure.Binding.InfrastructureNinjectBinding(),
//                      new PostaFlya.Domain.Binding.CommandNinjectBinding(),
//                      new WebSite.Azure.Common.Binding.AzureCommonNinjectBinding(),
//                      new PostaFlya.DataRepository.Binding.AzureRepositoryNinjectBinding(c => c.InTransientScope()),
//                      new PostaFlya.DataRepository.Binding.TableNameNinjectBinding(),
//                      new WebSite.Application.Binding.ApplicationNinjectBinding(),
//                      new PostaFlya.Application.Domain.Binding.ApplicationDomainNinjectBinding(),
//                      new WebSite.Application.Azure.Binding.AzureApplicationNinjectBinding(),
//                  };
//    }
}
