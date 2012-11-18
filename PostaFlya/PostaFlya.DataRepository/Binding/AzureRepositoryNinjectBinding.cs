using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.Binding;
using Website.Azure.Common.Sql;
using Website.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Internal;
using PostaFlya.DataRepository.Search.Implementation;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util;
using Website.Domain.Browser.Query;

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
                          typeof(QueryServiceForBrowserAggregateInterface)
                      });
            _repositoryScopeConfiguration(kernel.Bind(typeof(GenericQueryServiceInterface))
                .To(typeof(JsonRepository)));
            _repositoryScopeConfiguration(kernel.Bind(typeof(GenericRepositoryInterface))
                .To(typeof(JsonRepository)));
            _repositoryScopeConfiguration(kernel.Bind(typeof(QueryServiceForBrowserAggregateInterface))
                .To(typeof(JsonRepositoryWithBrowser)));


            Trace.TraceInformation("Binding TableNameNinjectBinding");

//            Bind<AzureCommentRepository>().ToSelf();//this is only used inside other repositories so no need to configure scope etc
//            Bind<AzureClaimRepository>().ToSelf();
 
            //this basically names the azure table context so
            //we can set up bindings for the Type => TableName dictionary
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("flier");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("taskjob");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("image");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("browser");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("comments");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("claims");

            //Kernel.Bind<AzureTableContext>().ToSelf().Named("websiteinfo");

            

//            Bind<PropertyGroupTableSerializerInterface>().ToMethod(context 
//                => new DefaultPropertyGroupTableSerializer(CustomEdmSerializers.CustomSerializers)
//                ).InSingletonScope();

            kernel.BindCommandHandlersFromCallingAssembly(syntax => syntax.InTransientScope());
            kernel.BindSubscribersFromCallingAssembly(syntax => syntax.InTransientScope());
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
