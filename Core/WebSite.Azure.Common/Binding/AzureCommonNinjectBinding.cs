﻿using System.Configuration;
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
using Website.Infrastructure.Configuration;

namespace Website.Azure.Common.Binding
{
    public class AzureCommonNinjectBinding : NinjectModule
    {

        public override void Load()
        {
            Trace.TraceInformation("Binding AzureCommonNinjectBinding");

            Bind<ConfigurationServiceInterface>()
                .To<AzureConfigurationService>()
                .InSingletonScope();
            Config.Instance = Kernel.Get<ConfigurationServiceInterface>();

            //don't know where to put this, but can just go here for now
            //seems to have disappeared from 2.0
//            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
//            configSetter(Config.Instance.GetSetting(configName)));

            

            Bind<CloudStorageAccount>()
                .ToMethod(ctx =>
                    (RoleEnvironment.IsAvailable) ? //in the cloud (dev or real)
                    CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"))
                    :
                        AzureEnv.UseRealStorage ? 
                            CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"])
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

            Trace.TraceInformation("Finished Binding AzureCommonNinjectBinding");

        }
    }
}
