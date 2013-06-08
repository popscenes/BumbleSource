using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Analytic;
using Website.Application.Domain.TinyUrl;
using Website.Application.Schedule;
using Website.Azure.Common.TableStorage;
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

            tableNameProv.Add<BoardInterface>("board", e => e.Id);
            //tableNameProv.Add<BoardInterface>(JsonRepository.FriendlyIdPartiton, "boardFriendly", e => e.FriendlyId, e => e.Id);
            //tableNameProv.Add<BoardInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "boardByBrowser", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<BoardFlierInterface>("boardflier", e => e.Id);
            //tableNameProv.Add<BoardFlierInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "boardflierByBoard", e => e.AggregateId, e => e.Id.ToDescendingTimeKey(e.DateAdded));


            tableNameProv.Add<FlierInterface>("flier", e => e.Id);
            //tableNameProv.Add<FlierInterface>(JsonRepository.FriendlyIdPartiton, "flierFriendly", e => e.FriendlyId, e => e.Id);
            //tableNameProv.Add<FlierInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "flierByBrowser", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<BrowserInterface>("browser", e => e.Id);
//            tableNameProv.Add<BrowserInterface>(JsonRepository.FriendlyIdPartiton, "browserFriendly", e => e.FriendlyId, e => e.Id);
//            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.IdPartition, "browsercreds", e => e.ToUniqueString(), e => e.BrowserId);
//            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.AggregateIdPartition, "browsercredsByBrowser", e => e.BrowserId, e => e.ToUniqueString());

            tableNameProv.Add<ImageInterface>("image", e => e.Id);
//            tableNameProv.Add<ImageInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "imageByBrowser", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<ClaimInterface>("claim", e => e.Id);
//            tableNameProv.Add<ClaimInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "claimByBrowser", e => e.BrowserId, e => e.Id);
//            tableNameProv.Add<ClaimInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "claimByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<CommentInterface>("comment", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<CommentInterface>(JsonRepository.AggregateIdPartition, "commentByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<FlierBehaviourInterface>("flierbehaviour", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<FlierBehaviourInterface>(JsonRepository.FriendlyIdPartiton, "flierbehaviourFriendly", e => e.FriendlyId, e => e.Id);

            tableNameProv.Add<PaymentTransaction>("paymentTransaction", e => e.Id);
//            tableNameProv.Add<PaymentTransaction>(JsonRepository.AggregateIdPartition, "paymentTransactionByAggregate", e => e.AggregateId, e => e.Id.ToDescendingTimeKey(e.Time));

            tableNameProv.Add<CreditTransaction>("creditTransaction", e => e.Id);
//            tableNameProv.Add<CreditTransaction>(JsonRepository.AggregateIdPartition, "creditTransactionByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<TinyUrlRecordInterface>("tinyurlentity", e => e.Id);
//            tableNameProv.Add<TinyUrlRecordInterface>(JsonRepository.AggregateIdPartition, "tinyurlentityByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<JobBase>("jobs", e => e.Id);

            tableNameProv.Add<FlierAnalyticInterface>("analytics", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<FlierAnalyticInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "analyticsByAggregate", e => e.AggregateId, e => e.Id.ToAscendingTimeKey(e.Time));
//            tableNameProv.Add<FlierAnalyticInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "analyticsByBrowser", e => e.BrowserId, e => e.Id);

            var tctx = Kernel.Get<TableContextInterface>();
            foreach (var tableName in tableNameProv.GetAllTableNames())
            {
                tctx.InitTable<JsonTableEntry>(tableName);    
            }

            Trace.TraceInformation("Finished Binding TableNameNinjectBinding");

        }

        #endregion
    }
}
