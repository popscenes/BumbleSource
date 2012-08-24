using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Ninject;
using Ninject.Modules;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Behaviour.TaskJob;
using PostaFlya.DataRepository.Browser;
using PostaFlya.DataRepository.Content;
using PostaFlya.DataRepository.Flier;
using PostaFlya.DataRepository.Internal;
using WebSite.Infrastructure.Domain;
using Website.Domain.Browser;
using Website.Domain.Comments;
using Website.Domain.Content;
using Website.Domain.Likes;

namespace PostaFlya.DataRepository.Binding
{
    public class TableNameNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding TableNameNinjectBinding");

            var tableNameProv = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            tableNameProv.Add<FlierInterface>(JsonRepositoryWithBrowser.IdPartition, "flier", e => e.Id);
            tableNameProv.Add<FlierInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "flier", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<BrowserInterface>(JsonRepository.IdPartition, "browser", e => e.Id);
            tableNameProv.Add<BrowserInterface>(AzureBrowserRepository.HandlePartitionId, "browser", e => e.Handle, e => e.Id);
            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.IdPartition, "browsercreds", e => e.GetHash(), e => e.BrowserId);
            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.AggregateIdPartition, "browsercreds", e => e.BrowserId, e => e.GetHash());

            tableNameProv.Add<ImageInterface>(JsonRepository.IdPartition, "image", e => e.Id);
            tableNameProv.Add<ImageInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "image", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<LikeInterface>(JsonRepositoryWithBrowser.IdPartition, "like", e => e.Id);
            tableNameProv.Add<LikeInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "like", e => e.BrowserId, e => (DateTime.MaxValue.Ticks - e.LikeTime.Ticks).ToString("D20") + e.Id);
            tableNameProv.Add<LikeInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "like", e => e.AggregateId, e => e.LikeTime.Ticks.ToString("D20") + e.Id);

            tableNameProv.Add<CommentInterface>(JsonRepository.IdPartition, "comment", e => e.Id, e => e.CommentTime.Ticks.ToString("D20") + e.Id);
            tableNameProv.Add<CommentInterface>(JsonRepository.AggregateIdPartition, "comment", e => e.AggregateId, e => e.CommentTime.Ticks.ToString("D20") + e.Id);

            tableNameProv.Add<FlierBehaviourInterface>(JsonRepository.IdPartition, "flierbehaviour", e => e.Id);

            var tctx = Kernel.Get<TableContextInterface>();
            foreach (var tableName in tableNameProv.GetAllTableNames())
            {
                tctx.InitTable<JsonTableEntry>(tableName);    
            }

            //table name bindings the old way to be used with AzureTableContext
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(new TableNameAndPartitionProviderCollection()
//                                {
//                                    BrowserStorageDomain.TableNamesAndPartition,
//                                    BrowserIdentityProviderCredential.TableNamesAndPartition
//                                })
//                .WhenAnyAnchestorNamed("browser");
//
//
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(FlierStorageDomain.TableNamesAndPartition)
//                .WhenAnyAnchestorNamed("flier");
//
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(ImageStorageDomain.TableNamesAndPartition)
//                .WhenAnyAnchestorNamed("image");
//
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(TaskJobStorageDomain.TableNamesAndPartition)
//                .WhenAnyAnchestorNamed("taskjob");
//
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(CommentStorageDomain.TableNamesAndPartition)
//                .WhenAnyAnchestorNamed("comments");
//
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//               .ToConstant(LikeStorageDomain.TableNamesAndPartition)
//               .WhenAnyAnchestorNamed("likes");

            Trace.TraceInformation("Finished Binding TableNameNinjectBinding");

        }

        #endregion
    }
}
