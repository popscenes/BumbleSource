using System.Diagnostics;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using Website.Application.Caching.Query;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Caching.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace Website.Application.Domain.Binding
{
    public class ApplicationDomainRepositoriesNinjectBinding : NinjectModule
    {
        private readonly ConfigurationAction _repositoryConfiguration;

        public ApplicationDomainRepositoriesNinjectBinding(ConfigurationAction repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }

        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationDomainRepositoriesNinjectBinding");
            //command handlers
            var kernel = Kernel as StandardKernel;
            //cached repositories
            kernel.BindRepositoriesFromCallingAssembly(_repositoryConfiguration
                , new[]
                      {
                          typeof(GenericQueryServiceInterface),
                          typeof(GenericRepositoryInterface)
                      });

            var binding = Rebind(typeof (GenericQueryServiceInterface))
                                .To(typeof (TimedExpiryCachedQueryService));
            _repositoryConfiguration(binding);

            binding = Rebind(typeof(GenericRepositoryInterface))
                .To(typeof (CachedRepositoryBase));
            _repositoryConfiguration(binding);
            
            Trace.TraceInformation("Finished Binding ApplicationDomainRepositoriesNinjectBinding");

        }
    }
}
