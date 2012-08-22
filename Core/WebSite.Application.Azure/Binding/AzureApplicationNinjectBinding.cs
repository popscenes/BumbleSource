using System.Diagnostics;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using Ninject.Modules;
using WebSite.Application.Azure.Command;
using WebSite.Application.Azure.Communication;
using WebSite.Application.Azure.Content;
using WebSite.Application.Binding;
using WebSite.Application.Command;
using WebSite.Application.Communication;
using WebSite.Application.Content;
using WebSite.Application.WebsiteInformation;
using WebSite.Azure.Common.TableStorage;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Util;
using WebSite.Application.Azure.WebsiteInformation;
using WebSite.Infrastructure.Binding;

namespace WebSite.Application.Azure.Binding
{
    public class AzureApplicationNinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Trace.TraceInformation("Binding AzureApplicationNinjectBinding");

            Bind<CommandQueueFactoryInterface>()
                .To<AzureCommandQueueFactory>()
                .InThreadScope();

            
            //worker command queue
            Kernel.Bind<CommandBusInterface>().ToMethod(
                ctx =>
                ctx.Kernel.Get<CommandQueueFactoryInterface>()
                    .GetCommandBusForEndpoint("workercommandqueue")
            )
            .WhenTargetHas<WorkerCommandBusAttribute>();

            Kernel.Bind<QueuedCommandScheduler>().ToMethod(
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

            var tableNameProv = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
            Kernel.Bind<BroadcastRegistratorInterface>().To<AzureBroadcastRegistrator>();
            //            Kernel.Bind<AzureTableContext>().ToSelf().Named("broadcastCommunicators");
            //            Kernel.Bind<TableNameAndPartitionProviderInterface>()
            //                .ToConstant(AzureBroadcastRegistrator.TableNameBinding)
            //                .WhenAnyAnchestorNamed("broadcastCommunicators");
            tableNameProv.Add<AzureBroadcastRegistration>(0, "broadcastCommunicators", e => "", e => e.Get<string>("Endpoint"));

            Kernel.Bind<WebsiteInfoServiceInterface>().To<WebsiteInfoServiceAzure>().WhenTargetHas<SourceDataSourceAttribute>();         
            tableNameProv.Add<WebsiteInfoEntity>(0, "websiteinfo", e => "", e => e.Get<string>("url"));
//            Kernel.Bind<AzureTableContext>().ToSelf().Named("websiteinfo");
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//               .ToConstant(WebsiteInfoServiceAzure.TableNameBinding)
//               .WhenAnyAnchestorNamed("websiteinfo");


            //need to call Kernel.Get<AzureInitCreateQueueAndBlobs>().Init();
            //from startup code somewhere
            Bind<InitServiceInterface>().To<AzureInitCreateQueueAndBlobs>().WithMetadata("storageinit", true);

            Trace.TraceInformation("Finished Binding AzureApplicationNinjectBinding");

        }
    }
}
