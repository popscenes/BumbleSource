using System.Diagnostics;
using Microsoft.Web.Administration;
using Microsoft.WindowsAzure.ServiceRuntime;
using PostaFlya.CommandWorker;
using Website.Azure.Common.Environment;

namespace PostaFlya
{
    public class WebRole : RoleEntryPoint
    {

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
            return ret;
        }



        public WebRole()
        {
        }

        public override void Run()
        {
            if (AzureEnv.IsRunningInProdFabric())
            {
                //turn app pool to always on and pre-load enabled
                using (var serverManager = new ServerManager())
                {
                    var mainSite = serverManager.Sites[RoleEnvironment.CurrentRoleInstance.Id + "_Web"];
                    var mainApplication = mainSite.Applications["/"];
                    mainApplication["preloadEnabled"] = true;

                    var mainApplicationPool = serverManager.ApplicationPools[mainApplication.ApplicationPoolName];
                    mainApplicationPool["startMode"] = "AlwaysRunning";
                    mainApplicationPool.AutoStart = true;
                    serverManager.CommitChanges();
                }    
            }

            
            base.Run();           
        }

        public override void OnStop()
        {
  
            base.OnStop();
        }
    }
}
