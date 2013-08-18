using System.Diagnostics;
using Microsoft.WindowsAzure.Storage.Blob;
using Ninject;
using Ninject.Modules;
using Website.Application.Azure.Command;
using Website.Application.Azure.Content;
using Website.Application.Azure.Queue;
using Website.Application.Azure.Util;
using Website.Application.Azure.WebsiteInformation;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Messaging;
using Website.Application.Queue;
using Website.Application.Util;
using Website.Application.WebsiteInformation;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Util;
using Website.Infrastructure.Util.Extension;

namespace Website.Application.Azure.Binding
{
    public class AzureApplicationNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding AzureApplicationNinjectBinding");


            //queue factory
            Kernel.Bind<QueueFactoryInterface>()
                    .ToMethod(context => 
                        new ServiceBusQueueFactory(context.Kernel.Get<ConfigurationServiceInterface>()
                            , "ServiceBusNamespace"
                            , AzureEnv.IsRunningInProdFabric() 
                                ? null //this is just so in dev everyone has their own queues
                                : System.Environment.MachineName.ToLowerHiphen())
                        )
                  //.To<ServiceBusQueueFactory>()
                  //.To<AzureCloudQueueFactory>()
                  .InThreadScope();

            Bind<MessageQueueFactoryInterface>()
                .To<AzureMessageQueueFactory>()
                .InThreadScope();
            
            //worker command queue
            Kernel.Bind<MessageBusInterface>().ToMethod(
                ctx =>
                ctx.Kernel.Get<MessageQueueFactoryInterface>()
                   .GetMessageBusForEndpoint("workercommandqueue")
                )
                .WhenTargetHas<WorkerCommandBusAttribute>();
            //.InThreadScope();

            Kernel.Bind<QueuedMessageProcessor>().ToMethod(
                ctx =>
                ctx.Kernel.Get<MessageQueueFactoryInterface>()
                    .GetProcessorForEndpoint("workercommandqueue")
                )
                .WithMetadata("workercommandqueue", true);
            //worker command queue


            //image storage
            Kernel.Bind<BlobStorageInterface>().ToMethod(
                ctx => new AzureCloudBlobStorage(ctx.Kernel.Get<CloudBlobContainer>(bm => bm.Has("imagestorage"))))
                .WhenTargetHas<ImageStorageAttribute>();

            Kernel.Bind<CloudBlobContainer>().ToMethod(
                ctx => ctx.Kernel.Get<CloudBlobClient>().GetContainerReference("imagestorage"))
                .WithMetadata("imagestorage", true);

            //end image storage

            //applicationstorage
            Kernel.Bind<BlobStorageInterface>().ToMethod(
                ctx => new AzureCloudBlobStorage(ctx.Kernel.Get<CloudBlobContainer>(bm => bm.Has("applicationstorage"))))
                .WhenTargetHas<ApplicationStorageAttribute>();

            Kernel.Bind<CloudBlobContainer>().ToMethod(
                ctx => ctx.Kernel.Get<CloudBlobClient>().GetContainerReference("applicationstorage"))
                .WithMetadata("applicationstorage", true);
            //end applicationstorage

            var tableNameProv = Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            Kernel.Bind<WebsiteInfoServiceInterface>().To<WebsiteInfoServiceAzure>().WhenTargetHas<SourceDataSourceAttribute>();
            tableNameProv.Add<WebsiteInfoEntity>("websiteinfoEntity", e => e.Get<string>("url"));


            //need to call Kernel.Get<AzureInitCreateQueueAndBlobs>().Init();
            //from startup code somewhere
            Bind<InitServiceInterface>().To<AzureInitCreateQueueAndBlobs>().WithMetadata("storageinit", true);

            Bind<TempFileStorageInterface>().To<AzureTempFileStorage>();

            Trace.TraceInformation("Finished Binding AzureApplicationNinjectBinding");

        }
    }
}
