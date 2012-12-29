using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Internal;
using Website.Domain.Browser;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Domain.Content;
using Website.Domain.Payment;

namespace PostaFlya.DataRepository.Binding
{
    public class TableNameNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding TableNameNinjectBinding");

            var tableNameProv = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            tableNameProv.Add<BoardInterface>(JsonRepositoryWithBrowser.IdPartition, "board", e => e.Id);
            tableNameProv.Add<BoardInterface>(JsonRepository.FriendlyIdPartiton, "board", e => e.FriendlyId, e => e.Id);
            tableNameProv.Add<BoardInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "board", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<BoardFlierInterface>(JsonRepositoryWithBrowser.IdPartition, "boardflier", e => e.Id);
            tableNameProv.Add<BoardFlierInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "boardflier", e => e.AggregateId, e => e.Id.ToDescendingTimeKey(e.DateAdded));


            tableNameProv.Add<FlierInterface>(JsonRepositoryWithBrowser.IdPartition, "flier", e => e.Id);
            tableNameProv.Add<FlierInterface>(JsonRepository.FriendlyIdPartiton, "flier", e => e.FriendlyId, e => e.Id);
            tableNameProv.Add<FlierInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "flier", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<BrowserInterface>(JsonRepository.IdPartition, "browser", e => e.Id);
            tableNameProv.Add<BrowserInterface>(JsonRepository.FriendlyIdPartiton, "browser", e => e.FriendlyId, e => e.Id);
            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.IdPartition, "browsercreds", e => e.GetHash(), e => e.BrowserId);
            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.AggregateIdPartition, "browsercreds", e => e.BrowserId, e => e.GetHash());

            tableNameProv.Add<ImageInterface>(JsonRepository.IdPartition, "image", e => e.Id);
            tableNameProv.Add<ImageInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "image", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<ClaimInterface>(JsonRepositoryWithBrowser.IdPartition, "claim", e => e.Id);
            tableNameProv.Add<ClaimInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "claim", e => e.BrowserId, e => e.Id);
            tableNameProv.Add<ClaimInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "claim", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<CommentInterface>(JsonRepository.IdPartition, "comment", e => e.Id);
            tableNameProv.Add<CommentInterface>(JsonRepository.AggregateIdPartition, "comment", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<FlierBehaviourInterface>(JsonRepository.IdPartition, "flierbehaviour", e => e.Id);
            tableNameProv.Add<FlierBehaviourInterface>(JsonRepository.FriendlyIdPartiton, "flierbehaviour", e => e.FriendlyId, e => e.Id);

            tableNameProv.Add<PaymentTransaction>(JsonRepository.IdPartition, "paymentTransaction", e => e.Id);
            tableNameProv.Add<PaymentTransaction>(JsonRepository.AggregateIdPartition, "paymentTransaction", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<CreditTransaction>(JsonRepository.IdPartition, "creditTransaction", e => e.Id);
            tableNameProv.Add<CreditTransaction>(JsonRepository.AggregateIdPartition, "creditTransaction", e => e.AggregateId, e => e.Id);


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
//               .ToConstant(ClaimStorageDomain.TableNamesAndPartition)
//               .WhenAnyAnchestorNamed("claims");

            Trace.TraceInformation("Finished Binding TableNameNinjectBinding");

        }

        #endregion
    }
}
