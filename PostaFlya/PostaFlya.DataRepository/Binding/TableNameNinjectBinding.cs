using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using PostaFlya.DataRepository.DomainQuery;
using PostaFlya.DataRepository.Indexes;
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
using Website.Domain.Location;
using Website.Domain.Payment;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Util.Extension;


namespace PostaFlya.DataRepository.Binding
{
    public class TableNameNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding TableNameNinjectBinding");

            var tableNameProv = Kernel.Get<TableNameAndIndexProviderServiceInterface>();


            tableNameProv.AddIndex("tinyUrlIDX", new TinyUrlIndex<EntityWithTinyUrlInterface>());

            tableNameProv.Add<BoardInterface>("boardEntity", e => e.Id);
            tableNameProv.AddIndex("boardIDX", new FriendlyIdIndexDefinition<BoardInterface>());
            tableNameProv.AddIndex("boardAdminEmailIDX", new BoardAdminEmailIndex());
            tableNameProv.AddIndex("textSearchIDX", new BoardTextSearchIndex());
            tableNameProv.AddIndex("boardSuburbIDX", new BoardSuburbIndex());


            tableNameProv.Add<FlierInterface>("flierEntity", e => e.Id);
            tableNameProv.AddIndex("flierFriendlyIDX", new FriendlyIdIndexDefinition<FlierInterface>());
            tableNameProv.AddIndex("flierBrowserIDX", new BrowserIdIndex<FlierInterface>());
            tableNameProv.AddIndex("flyerSuburbIDX", new FlyerSuburbIndex());
            tableNameProv.AddIndex("flyerBoardIDX", new FlyerBoardsIndex());
            
            tableNameProv.Add<BrowserInterface>("browserEntity", e => e.Id);
            tableNameProv.AddIndex("browserFriendlyIDX", new FriendlyIdIndexDefinition<BrowserInterface>());
            tableNameProv.AddIndex("browserCredIDX", new BrowserCredentialIndex());
            tableNameProv.AddIndex("browserEmailIDX", new BrowserEmailSelector());

            tableNameProv.Add<ImageInterface>("imageEntity", e => e.Id);
            tableNameProv.AddIndex("imageBrowserIDX", new BrowserIdIndex<ImageInterface>());

            tableNameProv.Add<ClaimInterface>("claimEntity", e => e.AggregateId, e => e.Id);
            tableNameProv.AddIndex("claimBrowserIDX", new BrowserIdForAggregateMemberIndex<ClaimInterface>());

            tableNameProv.Add<CommentInterface>("commentEntity", e => e.AggregateId, e => e.Id);


            tableNameProv.Add<PaymentTransaction>("paymentTransactionEntity", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<CreditTransaction>("creditTransactionEntity", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<TinyUrlRecord>("tinyurlentityEntity", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<JobBase>("jobsEntity", e => e.Id);

            tableNameProv.Add<FlierAnalyticInterface>("analyticsEntity", e => e.Id);

            tableNameProv.Add<SuburbEntityInterface>("suburbEntity", e => e.Id);
            tableNameProv.AddIndex("textSearchIDX", new SuburbTextSearchIndex());
            tableNameProv.AddIndex("geoSearchIDX", new SuburbGeoSearchIndex());


            Trace.TraceInformation("TableNameNinjectBinding Initializing Tables");

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
