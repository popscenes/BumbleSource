using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Modules;
using PostaFlya.DataRepository.DomainQuery;
using PostaFlya.DataRepository.DomainQuery.Browser;
using PostaFlya.DataRepository.Search.Event;
using Website.Azure.Common.Binding;
using Website.Azure.Common.Sql;
using Website.Azure.Common.Sql.Infrastructure;
using Website.Azure.Common.TableStorage;
//using PostaFlya.DataRepository.Search.Implementation;
using Website.Domain.Comments;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util;
using TypeUtil = Website.Infrastructure.Types.TypeUtil;

namespace PostaFlya.DataRepository.Binding
{
    public class AzureRepositoryNinjectBinding : NinjectModule
    {
        private readonly ConfigurationAction _repositoryScopeConfiguration;


        public AzureRepositoryNinjectBinding(ConfigurationAction repositoryScopeConfiguration = null)
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
            kernel.BindRepositoriesFromCallingAssembly(
                _repositoryScopeConfiguration ?? (syntax => syntax.InTransientScope()) 
                , new []
                      {
                          typeof(GenericQueryServiceInterface),
                          typeof(GenericRepositoryInterface),

                      });
//            _repositoryScopeConfiguration(kernel.Bind(typeof(GenericQueryServiceInterface))
//                .To(typeof(JsonRepository)));
//            _repositoryScopeConfiguration(kernel.Bind(typeof(GenericRepositoryInterface))
//                .To(typeof(JsonRepository)));

            Unbind<UnitOfWorkInterface>();
            Unbind<UnitOfWorkForRepoInterface>();
            Rebind<UnitOfWorkInterface, UnitOfWorkForRepoInterface, UnitOfWorkForRepoJsonRepository>()
                .To<UnitOfWorkForRepoJsonRepository>().InThreadScope();
            Rebind<JsonRepository>().ToSelf().InTransientScope();

            if (_repositoryScopeConfiguration != null)//new scope for repos
            {
                var config = _repositoryScopeConfiguration + (syntax => syntax.InTransientScope());

                var binding = Rebind(typeof(GenericQueryServiceInterface))
                    .ToMethod<object>(context =>
                              context.Kernel.Get<UnitOfWorkForRepoInterface>().CurrentQuery);
                config(binding);

                binding = Rebind(typeof(GenericRepositoryInterface))
                      .ToMethod<object>(context =>
                                        context.Kernel.Get<UnitOfWorkForRepoInterface>().CurrentRepo);
                config(binding);
            }


            kernel.BindMessageAndQueryHandlersFromCallingAssembly(syntax => syntax.InTransientScope());

            kernel.BindGenericHandlersForTypesFrom(_repositoryScopeConfiguration
                    , Assembly.GetAssembly(typeof(Comment)) 
                    , Assembly.GetAssembly(typeof (PostaFlya.Domain.Flier.Flier)));

            //kernel.BindGenericQueryHandlersFromCallingAssemblyForTypesFrom(Assembly.GetAssembly(typeof(PostaFlya.DataRepository.DomainQuery.Board.FindBoardByAdminEmailQueryHandler))
              //  ,  _repositoryScopeConfiguration);

            //Bind<QueryHandlerInterface<FindBoardByAdminEmailQuery, List<Domain.Boards.Board>>>()
             //   .To<FindBoardByAdminEmailQueryHandler>();

            

            //kernel.BindGenericQueryHandlersFromCallingAssemblyForTypesFrom(Assembly.GetAssembly(typeof(PostaFlya.Domain.Boards.Board))
             //   , _repositoryScopeConfiguration);

            kernel.BindEventHandlersFromCallingAssembly(syntax => syntax.InTransientScope());
//            Bind<FlierSearchServiceInterface>()
//                .To<SqlFlierSearchService>();

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
                       var configDb = Config.Instance.GetSetting("SearchDbConnectionStringDbName");
                       masterConn.InitialCatalog = string.IsNullOrWhiteSpace(configDb) ? "SearchDb" : configDb;
                       return masterConn.ToString();
                    }
            )
            .WhenTargetHas<SqlSearchConnectionString>()
            .InSingletonScope();

            Trace.TraceInformation("Finished Binding AzureRepositoryNinjectBinding");
 
        }



    }

    public static class AzureRepositoryNinjectBindingExtensions
    {
        public static void BindGenericHandlersForTypesFrom(
            this IKernel kernel, ConfigurationAction ninjectConfiguration
            , params Assembly[] typeAssemblies)
        {

            var qh = typeof(FindByFriendlyIdQueryHandler<>);
            var qhExp = TypeUtil.GetExpandedTypesUsing(qh, typeAssemblies);
            foreach (var inst in qhExp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

            qh = typeof(GetAggregateByBrowserIdQueryHandler<>);
            qhExp = TypeUtil.GetExpandedTypesUsing(qh, typeAssemblies);
            foreach (var inst in qhExp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

            qh = typeof(FindBrowserByIdentityProviderQueryHandler<>);
            qhExp = TypeUtil.GetExpandedTypesUsing(qh, typeAssemblies);
            foreach (var inst in qhExp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

            qh = typeof(GetByBrowserIdQueryHandler<>);
            qhExp = TypeUtil.GetExpandedTypesUsing(qh, typeAssemblies);
            foreach (var inst in qhExp)
            {
                kernel.BindToGenericInterface(inst, typeof(QueryHandlerInterface<,>));
            }

            var ev = typeof(EntityIndexEventHandler<>);
            var evExp = TypeUtil.GetExpandedTypesUsing(ev, typeAssemblies);
            foreach (var inst in evExp)
            {
                kernel.BindToGenericInterface(inst, typeof(HandleEventInterface<>));
            }

        }
    }
}
