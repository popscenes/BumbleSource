using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using Website.Application.Caching.Query;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Caching.Command;
using Website.Infrastructure.Caching.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Application.Domain.Query;
using Website.Domain.Browser.Query;

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
                          typeof(QueryServiceForBrowserAggregateInterface)
                      });

            Trace.TraceInformation("Finished Binding ApplicationDomainRepositoriesNinjectBinding");

        }
    }
}
