using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using PostaFlya.DataRepository.DomainQuery;
using PostaFlya.DataRepository.DomainQuery.Browser;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.Binding;
using Website.Azure.Common.Sql;
using Website.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Search.Implementation;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util;

namespace PostaFlya.DataRepository.Binding
{
    public class AzureRepositoryNinjectBinding : NinjectModule
    {
        private readonly ConfigurationAction _repositoryScopeConfiguration;


        public AzureRepositoryNinjectBinding(ConfigurationAction repositoryScopeConfiguration)
        {
            _repositoryScopeConfiguration = repositoryScopeConfiguration;
        }

        public override void Load()
        {
            Trace.TraceInformation("Binding AzureRepositoryNinjectBinding");
            //need to call from startup code somewhere
            Bind<InitServiceInterface>().To<AzureInitCreateTables>().WithMetadata("tablestorageinit", true);

            //Kernel.Bind<WebsiteInfoServiceInterface>().To<WebsiteInfoServiceAzure>().WhenTargetHas<SourceDataSourceAttribute>();

            var kernel = Kernel as StandardKernel;
            kernel.BindRepositoriesFromCallingAssembly(_repositoryScopeConfiguration
                , new []
                      {
                          typeof(GenericQueryServiceInterface),
                          typeof(GenericRepositoryInterface),

                      });
            _repositoryScopeConfiguration(kernel.Bind(typeof(GenericQueryServiceInterface))
                .To(typeof(JsonRepository)));
            _repositoryScopeConfiguration(kernel.Bind(typeof(GenericRepositoryInterface))
                .To(typeof(JsonRepository)));

            kernel.BindCommandAndQueryHandlersFromCallingAssembly(_repositoryScopeConfiguration);
            kernel.BindGenericQueryHandlersFromCallingAssemblyForTypesFrom(Assembly.GetAssembly(typeof(PostaFlya.Domain.Flier.Flier))
                , _repositoryScopeConfiguration);

            kernel.BindGenericQueryHandlersFromCallingAssemblyForTypesFrom(Assembly.GetAssembly(typeof(Website.Domain.Claims.Claim))
                ,  _repositoryScopeConfiguration);

            kernel.BindEventHandlersFromCallingAssembly(syntax => syntax.InTransientScope());
            Bind<FlierSearchServiceInterface>()
                .To<SqlFlierSearchService>();

            Bind<string>()
                .ToMethod(ctx => SqlExecute.GetConnectionStringFromConfig("SearchDbConnectionString")
                )
            .InSingletonScope()
            .WithMetadata("SqlMasterDbConnectionString", true);

            Bind<string>()
            .ToMethod(ctx
                => ctx.Kernel.Get<string>(c => c.Has("SqlMasterDbConnectionString"))
            )
            .WhenTargetHas<SqlMasterDbConnectionString>()
            .InSingletonScope();
            

            Bind<string>()
            .ToMethod(ctx 
                => {
                       var master = ctx.Kernel.Get<string>(c => c.Has("SqlMasterDbConnectionString"));
                       var masterConn = new SqlConnectionStringBuilder(master);
                       var configDb = ConfigurationManager.AppSettings["SearchDbConnectionStringDbName"];
                       masterConn.InitialCatalog = string.IsNullOrWhiteSpace(configDb) ? "SearchDb" : configDb;
                       return masterConn.ToString();
                    }
            )
            .WhenTargetHas<SqlSearchConnectionString>()
            .InSingletonScope();

            Trace.TraceInformation("Finished Binding AzureRepositoryNinjectBinding");
 
        }



    }
}
