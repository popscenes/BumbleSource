using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using PostaFlya.Application.Domain.Query;
using PostaFlya.Domain.Browser.Query;
using WebSite.Application.Caching.Query;
using WebSite.Infrastructure.Binding;
using WebSite.Infrastructure.Caching.Command;
using WebSite.Infrastructure.Caching.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Application.Domain.Binding
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
