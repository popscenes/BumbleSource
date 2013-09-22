using System;
using System.Diagnostics;
using Microsoft.Web.Administration;
using Microsoft.WindowsAzure.ServiceRuntime;
using PostaFlya.CommandWorker;
using Website.Azure.Common.Environment;
//using WindowsAzure.DevelopmentFabric.IISConfigurator.Syncronizer;

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

                Action<ServerManager> updatePreload = (serverManager) =>
                    {
                        try
                        {           

                            //turn app pool to always on and pre-load enabled
                            var mainSite = serverManager.Sites[RoleEnvironment.CurrentRoleInstance.Id + "_Web"];
                            var mainApplication = mainSite.Applications["/"];
                            mainApplication["preloadEnabled"] = true;

                            var mainApplicationPool =
                                serverManager.ApplicationPools[mainApplication.ApplicationPoolName];
                            mainApplicationPool["startMode"] = "AlwaysRunning";
                            mainApplicationPool.AutoStart = true;
                            mainApplicationPool.ProcessModel.IdleTimeout = new TimeSpan(0, 0, 00);
                            mainApplicationPool.Recycling.PeriodicRestart.Time = new TimeSpan(0,new Random().Next(1440, 1600), 0);

                            //force serve of gzip first time
                            var config = serverManager.GetApplicationHostConfiguration();
                            var serverRuntimeSection = config.GetSection("system.webServer/serverRuntime", "");
                            serverRuntimeSection["enabled"] = true;
                            serverRuntimeSection["frequentHitThreshold"] = 1;
                            serverRuntimeSection["frequentHitTimePeriod"] = TimeSpan.Parse("00:00:10");

                            serverManager.CommitChanges();
                            
                        }
                        catch (System.Exception e)
                        {

                            Trace.TraceError("Web Role On Start trace err {0} \n {1}", e.Message, e.StackTrace);
                            System.Diagnostics.EventLog.WriteEntry("WebRoleSource", e.Message + "\n" + e.StackTrace,
                                       System.Diagnostics.EventLogEntryType.Error);
                        }
                    };

               // ServerManagerBarrier.ApplyServerManagerActions(updatePreload); 
                using (var sm = new ServerManager())
                {
                    updatePreload(sm);
                }
                

            }

            try
            {
                base.Run();
            }
            catch (Exception e)
            {
                Trace.TraceError("Web Role On Start trace err {0} \n {1}", e.Message, e.StackTrace);
                System.Diagnostics.EventLog.WriteEntry("WebRoleSource", e.Message + "\n" + e.StackTrace,
                           System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public override void OnStop()
        {
  
            base.OnStop();
        }
    }
}
