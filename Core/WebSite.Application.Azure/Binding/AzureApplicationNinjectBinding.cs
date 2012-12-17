﻿using System.Diagnostics;
using Microsoft.WindowsAzure.Storage.Blob;
using Ninject;
using Ninject.Modules;
using Website.Application.ApplicationCommunication;
using Website.Application.Azure.Command;
using Website.Application.Azure.Communication;
using Website.Application.Azure.Content;
using Website.Application.Azure.Queue;
using Website.Application.Azure.WebsiteInformation;
using Website.Application.Binding;
using Website.Application.Command;
using Website.Application.Content;
using Website.Application.Queue;
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

            var tableNameProv = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
            Kernel.Bind<ApplicationBroadcastCommunicatorRegistrationInterface>().To<AzureApplicationBroadcastCommunicatorRegistration>();
            //            Kernel.Bind<AzureTableContext>().ToSelf().Named("broadcastCommunicators");
            //            Kernel.Bind<TableNameAndPartitionProviderInterface>()
            //                .ToConstant(AzureBroadcastRegistrator.TableNameBinding)
            //                .WhenAnyAnchestorNamed("broadcastCommunicators");
            tableNameProv.Add<AzureBroadcastRegistrationEntry>(0, "broadcastCommunicators", e => "", e => e.Get<string>("Endpoint"));

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
