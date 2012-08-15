using System;
using System.Diagnostics;
using System.Threading;
using System.Web.Hosting;

namespace WebSite.Common.Environment
{
    public class WebBackgroundWorker : IRegisteredObject
    {
        private readonly object _lock = new object();
        private readonly Action<CancellationToken> _run;
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Thread _thread;

        public WebBackgroundWorker(Action<CancellationToken> run)
        {
            _run = run;
            HostingEnvironment.RegisterObject(this);
            _thread = new Thread(ThreadStart) { IsBackground = true };          
        }

        public void Stop(bool immediate)
        {
            _cancellationTokenSource.Cancel();
            lock (_lock)//this just forces processing to finish
            {
                HostingEnvironment.UnregisterObject(this);
            }
            
        }

        public void Start()
        {
            _thread.Start();
        }

        private void ThreadStart()
        {
            lock (_lock)
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        _run(_cancellationTokenSource.Token);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("WebBackgroundWorker unexpected Error Restarting run action\n {0}, Stack {1}", e.Message, e.StackTrace);
                    }
                    
                }
                
            }
        }
    }
}
