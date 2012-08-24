using System.Diagnostics;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using WebSite.Application.Caching.Query;
using WebSite.Infrastructure.Binding;
using WebSite.Infrastructure.Caching.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;
using Website.Application.Domain.Query;
using Website.Domain.Browser.Query;

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
                          typeof(GenericRepositoryInterface),
                          typeof(QueryServiceWithBrowserInterface),
                          typeof(QueryByBrowserInterface)
                      });
            _repositoryConfiguration(kernel.Bind(typeof(GenericQueryServiceInterface))
                .To(typeof(TimedExpiryCachedQueryService)));
            _repositoryConfiguration(kernel.Bind(typeof(GenericRepositoryInterface))
                .To(typeof(CachedRepositoryBase)));
            _repositoryConfiguration(kernel.Bind(typeof(QueryServiceWithBrowserInterface))
                .To(typeof(CachedQueryServiceWithBrowser)));
            _repositoryConfiguration(kernel.Bind(typeof(QueryByBrowserInterface))
                .To(typeof(CachedQueryServiceWithBrowser)));

            

            Trace.TraceInformation("Finished Binding ApplicationDomainRepositoriesNinjectBinding");

        }
    }
}
