using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using PostaFlya.Application.Domain.Binding;
using Website.Application.Command;
using Website.Application.Schedule;
using Website.Azure.Common.Environment;

namespace PostaFlya.CommandWorker
{
    public class CommonWorkers
    {
        private static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      new Website.Infrastructure.Binding.InfrastructureNinjectBinding(),
                      new Website.Domain.Binding.DefaultServicesNinjectBinding(),
                      new Website.Domain.Binding.CommandNinjectBinding(),                 
                      new PostaFlya.Domain.Binding.DefaultServicesNinjectBinding(),
                      new PostaFlya.Domain.Binding.CommandNinjectBinding(),
                      new Website.Azure.Common.Binding.AzureCommonNinjectBinding(),
                      new DataRepository.Binding.AzureRepositoryNinjectBinding(c => c.InTransientScope()),
                      new DataRepository.Binding.TableNameNinjectBinding(),
                      new Website.Application.Binding.ApplicationCommandHandlersNinjectBinding(),
                      new Website.Application.Binding.ApplicationNinjectBinding(),
                      new Website.Application.Domain.Binding.ApplicationDomainNinjectBinding(),                
                      new Website.Application.Azure.Binding.AzureApplicationNinjectBinding(),
                       new PostaFlya.Application.Domain.Binding.ApplicationDomainNinjectBinding(),
                       new PostaFlya.Application.Domain.Binding.ApplicationDomainServicesNinjectBinding()
                  };

        private StandardKernel _kernel;
        private CancellationTokenSource _cancellationTokenSource;

        public void Init()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _kernel = new StandardKernel();
            _kernel.Load(NinjectModules);            
        }

        public void Run()
        {
            var task = new Task(() =>
                {
                    var processor = _kernel.Get<QueuedCommandProcessor>(ctx => ctx.Has("workercommandqueue"));
                    processor.Run(_cancellationTokenSource.Token);
                }, TaskCreationOptions.LongRunning);
            task.Start();

            //add other workers as above

            //only run scheduler on one instance
            if (AzureEnv.GetInstanceIndex() != 0) return;
            var schedtask = new Task(() =>
                {
                    var scheduler = _kernel.Get<SchedulerInterface>();
                    scheduler.Run(_cancellationTokenSource);
                }, TaskCreationOptions.LongRunning);
            schedtask.Start();            
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
