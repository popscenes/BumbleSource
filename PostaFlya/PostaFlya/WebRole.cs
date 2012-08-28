using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using PostaFlya.App_Start;
using Website.Application.Command;
using PostaFlya.CommandWorker;
using Microsoft.WindowsAzure.Diagnostics;

namespace PostaFlya
{
    public class WebRole : RoleEntryPoint
    {
        //if you want to share one web and worker roles for now uncomment = new QueuedCommandWorker();
//#if !DEBUG
//        private readonly QueuedCommandWorker _commandWorker = new QueuedCommandWorker();
//#else
        private readonly QueuedCommandWorker _commandWorker;// = new QueuedCommandWorker();
//#endif
        public override bool OnStart()
        {
//            var config = DiagnosticMonitor.GetDefaultInitialConfiguration();
//            config.WindowsEventLog.ScheduledTransferPeriod = System.TimeSpan.FromMinutes(1.0);
//            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", config);
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            System.Diagnostics.Trace.WriteLine("Web Role On Start trace writeln");
            Trace.TraceError("Web Role On Start trace err");
            Trace.TraceWarning("Web Role On Start trace warn");
            Trace.TraceInformation("Web Role On Start trace info");

            var ret =  base.OnStart();
            if(_commandWorker != null)
                _commandWorker.Init();
            return ret;
        }



        public WebRole()
        {
            if (_commandWorker != null)
                _commandWorker = new QueuedCommandWorker();
        }

        public override void Run()
        {
            base.Run();
            if (_commandWorker != null)
                _commandWorker.Run();            
        }

        public override void OnStop()
        {
            if (_commandWorker != null)
                _commandWorker.Stop();
            base.OnStop();
        }
    }
}
