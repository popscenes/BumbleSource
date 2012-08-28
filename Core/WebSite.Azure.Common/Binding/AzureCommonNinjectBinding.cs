using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using Ninject.Modules;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;

namespace Website.Azure.Common.Binding
{
    public class AzureCommonNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding AzureCommonNinjectBinding");

            //don't know where to put this, but can just go here for now
            if (RoleEnvironment.IsAvailable)//in the cloud (dev or real)
            {
                CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)));
            }


            Bind<CloudStorageAccount>()
                .ToMethod(ctx =>
                    (RoleEnvironment.IsAvailable) ? //in the cloud (dev or real)
                    CloudStorageAccount.FromConfigurationSetting("StorageConnectionString") :
                        AzureEnv.UseRealStorage ? 
                            CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"])
                            : CloudStorageAccount.Parse("UseDevelopmentStorage=true")
            );
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

            Bind<TableNameAndPartitionProviderServiceInterface>()
                .To<TableNameAndPartitionProviderService>()
                .InSingletonScope();

            Trace.TraceInformation("Finished Binding AzureCommonNinjectBinding");

        }
    }
}
