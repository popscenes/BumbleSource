using System.Configuration;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Ninject;
using Ninject.Modules;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace Website.Azure.Common.Binding
{
    public class AzureConfigNinjectBind  : NinjectModule
    {
        public override void Load()
        {
            var exist = Kernel.TryGet<ConfigurationServiceInterface>();
            if (exist == null || exist.GetType() != typeof (AzureConfigurationService))
            {
                Rebind<ConfigurationServiceInterface>()
                    .To<AzureConfigurationService>()
                    .InSingletonScope();
            }

            Config.Instance = Kernel.Get<ConfigurationServiceInterface>();
        }
    }
    public class AzureCommonNinjectBinding : NinjectModule
    {

        public override void Load()
        {
            Trace.TraceInformation("Binding AzureCommonNinjectBinding");

            //don't know where to put this, but can just go here for now
            //seems to have disappeared from 2.0
//            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
//            configSetter(Config.Instance.GetSetting(configName)));

            

            Bind<CloudStorageAccount>()
                .ToMethod(ctx =>
                        AzureEnv.UseRealStorage ?
                            CloudStorageAccount.Parse(Config.Instance.GetSetting("StorageConnectionString"))
                            : CloudStorageAccount.Parse("UseDevelopmentStorage=true")
            ).InThreadScope();

            //replace line above for fiddler
            //CloudStorageAccount.Parse("UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://ipv4.fiddler"));


            Bind<CloudQueueClient>()
                .ToMethod(ctx => ctx.Kernel.Get<CloudStorageAccount>().CreateCloudQueueClient());
            Bind<CloudBlobClient>()
                .ToMethod(ctx => ctx.Kernel.Get<CloudStorageAccount>().CreateCloudBlobClient());
            Bind<CloudTableClient>()
                .ToMethod(ctx => ctx.Kernel.Get<CloudStorageAccount>().CreateCloudTableClient());

            Bind<TableContextInterface>()
                .To<TableContext>()
                .InTransientScope();

            Bind<TableNameAndIndexProviderServiceInterface>()
                .To<TableNameAndIndexProviderService>()
                .InSingletonScope();
            Bind<TableIndexServiceInterface>()
                .To<TableIndexService>()
                .InTransientScope();

            Unbind<UnitOfWorkInterface>();
            Unbind<UnitOfWorkForRepoInterface>();
            Rebind<UnitOfWorkInterface, UnitOfWorkForRepoInterface, UnitOfWorkForRepoJsonRepository>()
                .To<UnitOfWorkForRepoJsonRepository>().InThreadScope();
            Bind<JsonRepository>().ToSelf().InTransientScope();

            Bind(typeof(GenericQueryServiceInterface))
                .ToMethod<object>(context =>
                          context.Kernel.Get<UnitOfWorkForRepoJsonRepository>().CurrentRepo)
                          .InTransientScope();

            Bind(typeof(GenericRepositoryInterface))
                .ToMethod<object>(context =>
                          context.Kernel.Get<UnitOfWorkForRepoJsonRepository>().CurrentRepo)
                          .InTransientScope();


            Trace.TraceInformation("Finished Binding AzureCommonNinjectBinding");

        }
    }
}
