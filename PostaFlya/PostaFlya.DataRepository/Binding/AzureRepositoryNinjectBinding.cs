using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using Ninject.Extensions.Conventions;
using Ninject.Planning.Bindings;
using Ninject.Syntax;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Likes;
using WebSite.Azure.Common.Binding;
using WebSite.Azure.Common.Environment;
using WebSite.Azure.Common.Sql;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Behaviour.TaskJob;
using PostaFlya.DataRepository.Browser;
using PostaFlya.DataRepository.Flier;
using PostaFlya.DataRepository.Internal;
using PostaFlya.DataRepository.Search.Implementation;
using PostaFlya.DataRepository.Search.Services;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Location;
using WebSite.Infrastructure.Binding;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;
using WebSite.Infrastructure.Util;

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
                          typeof(QueryServiceWithBrowserInterface),
                          typeof(QueryByBrowserInterface)
                      });
            _repositoryScopeConfiguration(kernel.Bind(typeof(GenericQueryServiceInterface))
                .To(typeof(JsonRepository)));
            _repositoryScopeConfiguration(kernel.Bind(typeof(GenericRepositoryInterface))
                .To(typeof(JsonRepository)));
            _repositoryScopeConfiguration(kernel.Bind(typeof(QueryServiceWithBrowserInterface))
                .To(typeof(JsonRepositoryWithBrowser)));
            _repositoryScopeConfiguration(kernel.Bind(typeof(QueryByBrowserInterface))
                .To(typeof(JsonRepositoryWithBrowser)));


            Trace.TraceInformation("Binding TableNameNinjectBinding");

//            Bind<AzureCommentRepository>().ToSelf();//this is only used inside other repositories so no need to configure scope etc
//            Bind<AzureLikeRepository>().ToSelf();
 
            //this basically names the azure table context so
            //we can set up bindings for the Type => TableName dictionary
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("flier");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("taskjob");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("image");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("browser");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("comments");
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("likes");

            //Kernel.Bind<AzureTableContext>().ToSelf().Named("websiteinfo");

            

            Bind<PropertyGroupTableSerializerInterface>().ToMethod(context 
                => new DefaultPropertyGroupTableSerializer(CustomEdmSerializers.CustomSerializers)
                ).InSingletonScope();

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
