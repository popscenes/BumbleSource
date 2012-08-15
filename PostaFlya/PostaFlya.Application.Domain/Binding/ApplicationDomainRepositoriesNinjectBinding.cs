using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using WebSite.Infrastructure.Binding;

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
            kernel.BindRepositoriesFromCallingAssembly(_repositoryConfiguration);

            

            Trace.TraceInformation("Finished Binding ApplicationDomainRepositoriesNinjectBinding");

        }
    }
}
