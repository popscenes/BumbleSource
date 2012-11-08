using System.Collections.Generic;
using System.Threading;
using Ninject;
using Ninject.Modules;
using Website.Application.Command;

namespace PostaFlya.CommandWorker
{
    public class QueuedCommandWorker
    {
        private static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      new Domain.Binding.DefaultServicesNinjectBinding(),
                      new Website.Infrastructure.Binding.InfrastructureNinjectBinding(),
                      new PostaFlya.Domain.Binding.CommandNinjectBinding(),
                      new Website.Azure.Common.Binding.AzureCommonNinjectBinding(),
                      new DataRepository.Binding.AzureRepositoryNinjectBinding(c => c.InTransientScope()),
                      new DataRepository.Binding.TableNameNinjectBinding(),
                      new Website.Application.Binding.ApplicationCommandHandlersNinjectBinding(),
                      new Website.Application.Binding.ApplicationNinjectBinding(),
                      new Application.Domain.Binding.ApplicationDomainNinjectBinding(),
                      new Website.Application.Azure.Binding.AzureApplicationNinjectBinding(),
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
            var processor = _kernel.Get<QueuedCommandProcessor>(ctx => ctx.Has("workercommandqueue"));
            processor.Run(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
