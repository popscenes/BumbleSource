using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Ninject;
using Ninject.Modules;
using PostaFlya.DataRepository.DomainQuery;
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
using Website.Domain.Query;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Types;
using Website.Infrastructure.Util.Extension;


namespace PostaFlya.DataRepository.Binding
{
    public static class DomainIndexSelectors
    {
        public const string BrowserIdIndex = "BrowserId";
        public const string BrowserCredentialIndex = "BrowserCredential";
        public const string BrowserEmailIndex = "BrowserEmail";
        public const string BoardAdminEmailIndex = "BoardAdminEmail";
        public const string TinyUrlIndex = "TinyUrl";

        public const string TextSearchIndex = "T";


        public static Expression<Func<EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> BrowserIdSelector<EntityInterfaceType>() where EntityInterfaceType : AggregateRootInterface, BrowserIdInterface
        {
            Expression<Func<EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory = root => new List<StorageTableKeyInterface>()
                {
                    new StorageTableKey()
                        {
                            PartitionKey = root.BrowserId.ToStorageKeySection(),
                            RowKey = root.Id.ToStorageKeySection()
                        }
                };
            return indexEntryFactory;
        }

        public static Expression<Func<EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> BrowserIdSelectorForAggregate<EntityInterfaceType>() where EntityInterfaceType : AggregateInterface, BrowserIdInterface
        {
            Expression<Func<EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory = root => new List<StorageTableKeyInterface>()
                {
                    new StorageTableKey()
                        {
                            PartitionKey = root.BrowserId.ToStorageKeySection(),
                            RowKey = root.AggregateId.ToStorageKeySection() + root.Id.ToStorageKeySection()
                        }
                };
            return indexEntryFactory;
        }

        public static Expression<Func<BrowserInterface, IEnumerable<StorageTableKeyInterface>>> BrowserCredentialSelector()
        {
            Expression<Func<BrowserInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory
                = browser =>

                  browser.ExternalCredentials.Select(
                      credential =>
                      new StorageTableKey()
                          {
                              PartitionKey = credential.ToUniqueString().ToStorageKeySection(),
                              RowKey = browser.Id.ToStorageKeySection()
                          });
                    
            return indexEntryFactory;
        }

        public static Expression<Func<BrowserInterface, IEnumerable<StorageTableKeyInterface>>> BrowserEmailSelector()
        {
            Expression<Func<BrowserInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory
                = browser =>

                  new List<StorageTableKeyInterface>() {new StorageTableKey()
                      {
                          PartitionKey = browser.EmailAddress.ToStorageKeySection(), 
                          RowKey = browser.Id.ToStorageKeySection()
                      }};
                    
            return indexEntryFactory;
        }

        public static Expression<Func<BoardInterface, IEnumerable<StorageTableKeyInterface>>> BaordAdminEmailSelector()
        {
            Expression<Func<BoardInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory
                = board =>
                  board.AdminEmailAddresses == null ? new List<StorageTableKey>() { new StorageTableKey() { RowKey = board.Id.ToStorageKeySection() } } : 
                  board.AdminEmailAddresses.Select(adimEmail => new StorageTableKey() { PartitionKey = adimEmail.ToStorageKeySection(), RowKey = board.Id.ToStorageKeySection() });

            return indexEntryFactory;
        }

        public static Expression<Func<EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>>
            TinyUrlSelector<EntityInterfaceType>() where EntityInterfaceType : EntityWithTinyUrlInterface, AggregateRootInterface
        {
            Expression<Func<EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory = root => new List<StorageTableKeyInterface>()
                {
                    new JsonTableEntry(new EntityKeyWithTinyUrl()
                        {
                            Id = root.Id, 
                            TinyUrl = root.TinyUrl, 
                            PrimaryInterface = root.PrimaryInterface, 
                            FriendlyId = root.FriendlyId
                        })
                        {
                           
                            PartitionKey = root.TinyUrl.ToStorageKeySection(),
                            RowKey = root.Id.ToStorageKeySection()
                        }
                };
            return indexEntryFactory;
        }

        public static Expression<Func<SuburbEntityInterface, IEnumerable<StorageTableKeyInterface>>> SuburbSearchSelector()
        {

            Expression<Func<SuburbEntityInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory
                =
                suburb => suburb.Locality
                    .ToTermsSearchKeys()
                    .Select((s, i) => new JsonTableEntry(new SearchEntityRecord()
                    {
                        Id = suburb.Id,
                        FriendlyId = suburb.FriendlyId,
                        TypeOfEntity = suburb.GetType().GetAssemblyQualifiedNameWithoutVer(),
                        DisplayString = suburb.GetSuburbDescription()
                    })
                    {

                        PartitionKey = s,
                        RowKey = i.ToRowKeyWithFieldWidth(10).ToStorageKeySection() + suburb.Id.ToStorageKeySection()
                    });


            return indexEntryFactory;
        }
    }

    public class TableNameNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding TableNameNinjectBinding");

            var tableNameProv = Kernel.Get<TableNameAndIndexProviderServiceInterface>();


            tableNameProv.AddIndex("tinyUrlIDX", DomainIndexSelectors.TinyUrlIndex, DomainIndexSelectors.TinyUrlSelector<EntityWithTinyUrlInterface>());

            tableNameProv.Add<BoardInterface>("boardEntity", e => e.Id);
            tableNameProv.AddIndex("boardIDX", StandardIndexSelectors.FriendlyIdIndex, StandardIndexSelectors.FriendlyIdSelector<BoardInterface>());
            tableNameProv.AddIndex("boardAdminEmailIDX", DomainIndexSelectors.BoardAdminEmailIndex, DomainIndexSelectors.BaordAdminEmailSelector());

            //tableNameProv.Add<BoardInterface>(JsonRepository.FriendlyIdPartiton, "boardFriendly", e => e.FriendlyId, e => e.Id);
            //tableNameProv.Add<BoardInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "boardByBrowser", e => e.BrowserId, e => e.Id);

            //tableNameProv.Add<BoardFlierInterface>("boardflierEntity", e => e.AggregateId, e => e.Id);
            //tableNameProv.Add<BoardFlierInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "boardflierByBoard", e => e.AggregateId, e => e.Id.ToDescendingTimeKey(e.DateAdded));


            tableNameProv.Add<FlierInterface>("flierEntity", e => e.Id);
            tableNameProv.AddIndex("flierFriendlyIDX", StandardIndexSelectors.FriendlyIdIndex, StandardIndexSelectors.FriendlyIdSelector<FlierInterface>());
            tableNameProv.AddIndex("flierBrowserIDX", DomainIndexSelectors.BrowserIdIndex, DomainIndexSelectors.BrowserIdSelector<FlierInterface>());
            


            //tableNameProv.Add<FlierInterface>(JsonRepository.FriendlyIdPartiton, "flierFriendly", e => e.FriendlyId, e => e.Id);
            //tableNameProv.Add<FlierInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "flierByBrowser", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<BrowserInterface>("browserEntity", e => e.Id);
            tableNameProv.AddIndex("browserFriendlyIDX", StandardIndexSelectors.FriendlyIdIndex, StandardIndexSelectors.FriendlyIdSelector<BrowserInterface>());
            tableNameProv.AddIndex("browserCredIDX", DomainIndexSelectors.BrowserCredentialIndex, DomainIndexSelectors.BrowserCredentialSelector());
            tableNameProv.AddIndex("browserEmailIDX", DomainIndexSelectors.BrowserEmailIndex, DomainIndexSelectors.BrowserEmailSelector());


//            tableNameProv.Add<BrowserInterface>(JsonRepository.FriendlyIdPartiton, "browserFriendly", e => e.FriendlyId, e => e.Id);
//            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.IdPartition, "browsercreds", e => e.ToUniqueString(), e => e.BrowserId);
//            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.AggregateIdPartition, "browsercredsByBrowser", e => e.BrowserId, e => e.ToUniqueString());

            tableNameProv.Add<ImageInterface>("imageEntity", e => e.Id);
            tableNameProv.AddIndex("imageBrowserIDX", DomainIndexSelectors.BrowserIdIndex, DomainIndexSelectors.BrowserIdSelector<ImageInterface>());

//            tableNameProv.Add<ImageInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "imageByBrowser", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<ClaimInterface>("claimEntity", e => e.AggregateId, e => e.Id);
            tableNameProv.AddIndex("claimBrowserIDX", DomainIndexSelectors.BrowserIdIndex, DomainIndexSelectors.BrowserIdSelectorForAggregate<ClaimInterface>());

//            tableNameProv.Add<ClaimInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "claimByBrowser", e => e.BrowserId, e => e.Id);
//            tableNameProv.Add<ClaimInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "claimByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<CommentInterface>("commentEntity", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<CommentInterface>(JsonRepository.AggregateIdPartition, "commentByAggregate", e => e.AggregateId, e => e.Id);


            tableNameProv.Add<PaymentTransaction>("paymentTransactionEntity", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<PaymentTransaction>(JsonRepository.AggregateIdPartition, "paymentTransactionByAggregate", e => e.AggregateId, e => e.Id.ToDescendingTimeKey(e.Time));

            tableNameProv.Add<CreditTransaction>("creditTransactionEntity", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<CreditTransaction>(JsonRepository.AggregateIdPartition, "creditTransactionByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<TinyUrlRecord>("tinyurlentityEntity", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<TinyUrlRecordInterface>(JsonRepository.AggregateIdPartition, "tinyurlentityByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<JobBase>("jobsEntity", e => e.Id);

            tableNameProv.Add<FlierAnalyticInterface>("analyticsEntity", e => e.Id);
//            tableNameProv.Add<FlierAnalyticInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "analyticsByAggregate", e => e.AggregateId, e => e.Id.ToAscendingTimeKey(e.Time));
//            tableNameProv.Add<FlierAnalyticInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "analyticsByBrowser", e => e.BrowserId, e => e.Id);


            tableNameProv.Add<SuburbEntityInterface>("suburbEntity", e => e.Id);
            tableNameProv.AddIndex<EntityInterface, SuburbEntityInterface>("textSearchIDX", DomainIndexSelectors.TextSearchIndex, DomainIndexSelectors.SuburbSearchSelector());


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
