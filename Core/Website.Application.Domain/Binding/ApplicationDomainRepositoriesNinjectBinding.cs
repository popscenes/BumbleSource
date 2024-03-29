﻿using System.Diagnostics;
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


            _repositoryConfiguration(kernel.Bind(typeof(GenericQueryServiceInterface))
                .To(typeof(TimedExpiryCachedQueryService)));

            _repositoryConfiguration(kernel.Bind(typeof(GenericRepositoryInterface))
                .To(typeof(CachedRepositoryBase)));
            
            Trace.TraceInformation("Finished Binding ApplicationDomainRepositoriesNinjectBinding");

        }
    }
}
