using System.Diagnostics;
using Microsoft.WindowsAzure.Storage.Blob;
using Ninject;
using Ninject.Modules;
using Website.Application.ApplicationCommunication;
using Website.Application.Azure.Command;
using Website.Application.Azure.Communication;
using Website.Application.Azure.Content;
using Website.Application.Azure.Queue;
using Website.Application.Azure.Util;
using Website.Application.Azure.WebsiteInformation;
using Website.Application.Binding;
using Website.Application.Command;
using Website.Application.Content;
using Website.Application.Queue;
using Website.Application.Util;
using Website.Application.WebsiteInformation;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util;

namespace Website.Application.Azure.Binding
{
    public class AzureApplicationNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding AzureApplicationNinjectBinding");

            Bind<CommandQueueFactoryInterface>()
                .To<AzureCommandQueueFactory>()
                .InThreadScope();

            //queue factory
            Kernel.Bind<QueueFactoryInterface>()
                  .To<AzureCloudQueueFactory>()
                  .InThreadScope();
            
            //worker command queue
            Kernel.Bind<CommandBusInterface>().ToMethod(
                ctx =>
                ctx.Kernel.Get<CommandQueueFactoryInterface>()
                    .GetCommandBusForEndpoint("workercommandqueue")
            )
            .WhenTargetHas<WorkerCommandBusAttribute>();

            Kernel.Bind<QueuedCommandProcessor>().ToMethod(
                ctx =>
                ctx.Kernel.Get<CommandQueueFactoryInterface>()
                    .GetSchedulerForEndpoint("workercommandqueue")
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
            Kernel.Bind<ApplicationBroadcastCommunicatorRegistrationInterface>().To<AzureApplicationBroadcastCommunicatorRegistration>();
            //            Kernel.Bind<AzureTableContext>().ToSelf().Named("broadcastCommunicators");
            //            Kernel.Bind<TableNameAndPartitionProviderInterface>()
            //                .ToConstant(AzureBroadcastRegistrator.TableNameBinding)
            //                .WhenAnyAnchestorNamed("broadcastCommunicators");
            tableNameProv.Add<AzureBroadcastRegistrationEntry>("broadcastCommunicators", e => e.Get<string>("Endpoint"));

            Kernel.Bind<WebsiteInfoServiceInterface>().To<WebsiteInfoServiceAzure>().WhenTargetHas<SourceDataSourceAttribute>();
            tableNameProv.Add<WebsiteInfoEntity>("websiteinfo", e => e.Get<string>("url"));
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("websiteinfo");
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//               .ToConstant(WebsiteInfoServiceAzure.TableNameBinding)
//               .WhenAnyAnchestorNamed("websiteinfo");


            //need to call Kernel.Get<AzureInitCreateQueueAndBlobs>().Init();
            //from startup code somewhere
            Bind<InitServiceInterface>().To<AzureInitCreateQueueAndBlobs>().WithMetadata("storageinit", true);

            Bind<TempFileStorageInterface>().To<AzureTempFileStorage>();

            Trace.TraceInformation("Finished Binding AzureApplicationNinjectBinding");

        }
    }
}
