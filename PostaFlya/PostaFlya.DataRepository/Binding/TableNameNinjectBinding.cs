using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
using Website.Domain.TinyUrl;
using Website.Infrastructure.Domain;


namespace PostaFlya.DataRepository.Binding
{
    public static class DomainIndexSelectors
    {
        public const string BrowserIdIndex = "BrowserId";
        public const string BrowserCredentialIndex = "BrowserCredential";
        public const string BrowserEmailIndex = "BrowserEmail";
        public const string TinyUrlIndex = "TinyUrl";


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
    }

    public class TableNameNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding TableNameNinjectBinding");

            var tableNameProv = Kernel.Get<TableNameAndIndexProviderServiceInterface>();


            tableNameProv.AddIndex("tinyUrlIndex", DomainIndexSelectors.TinyUrlIndex, DomainIndexSelectors.TinyUrlSelector<EntityWithTinyUrlInterface>());

            tableNameProv.Add<BoardInterface>("board", e => e.Id);
            tableNameProv.AddIndex("boardIndex", StandardIndexSelectors.FriendlyIdIndex, StandardIndexSelectors.FriendlyIdSelector<BoardInterface>());

            //tableNameProv.Add<BoardInterface>(JsonRepository.FriendlyIdPartiton, "boardFriendly", e => e.FriendlyId, e => e.Id);
            //tableNameProv.Add<BoardInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "boardByBrowser", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<BoardFlierInterface>("boardflier", e => e.AggregateId, e => e.Id);
            //tableNameProv.Add<BoardFlierInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "boardflierByBoard", e => e.AggregateId, e => e.Id.ToDescendingTimeKey(e.DateAdded));


            tableNameProv.Add<FlierInterface>("flier", e => e.Id);
            tableNameProv.AddIndex("flierFriendlyIndex", StandardIndexSelectors.FriendlyIdIndex, StandardIndexSelectors.FriendlyIdSelector<FlierInterface>());
            tableNameProv.AddIndex("flierBrowserIndex", DomainIndexSelectors.BrowserIdIndex, DomainIndexSelectors.BrowserIdSelector<FlierInterface>());
            


            //tableNameProv.Add<FlierInterface>(JsonRepository.FriendlyIdPartiton, "flierFriendly", e => e.FriendlyId, e => e.Id);
            //tableNameProv.Add<FlierInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "flierByBrowser", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<BrowserInterface>("browser", e => e.Id);
            tableNameProv.AddIndex("browserFriendlyIndex", StandardIndexSelectors.FriendlyIdIndex, StandardIndexSelectors.FriendlyIdSelector<BrowserInterface>());
            tableNameProv.AddIndex("browserCredIndex", DomainIndexSelectors.BrowserCredentialIndex, DomainIndexSelectors.BrowserCredentialSelector());
            tableNameProv.AddIndex("browserEmailIndex", DomainIndexSelectors.BrowserEmailIndex, DomainIndexSelectors.BrowserEmailSelector());


//            tableNameProv.Add<BrowserInterface>(JsonRepository.FriendlyIdPartiton, "browserFriendly", e => e.FriendlyId, e => e.Id);
//            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.IdPartition, "browsercreds", e => e.ToUniqueString(), e => e.BrowserId);
//            tableNameProv.Add<BrowserIdentityProviderCredential>(JsonRepositoryWithBrowser.AggregateIdPartition, "browsercredsByBrowser", e => e.BrowserId, e => e.ToUniqueString());

            tableNameProv.Add<ImageInterface>("image", e => e.Id);
            tableNameProv.AddIndex("imageBrowserIndex", DomainIndexSelectors.BrowserIdIndex, DomainIndexSelectors.BrowserIdSelector<ImageInterface>());

//            tableNameProv.Add<ImageInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "imageByBrowser", e => e.BrowserId, e => e.Id);

            tableNameProv.Add<ClaimInterface>("claim", e => e.AggregateId, e => e.Id);
            tableNameProv.AddIndex("claimBrowserIndex", DomainIndexSelectors.BrowserIdIndex, DomainIndexSelectors.BrowserIdSelectorForAggregate<ClaimInterface>());

//            tableNameProv.Add<ClaimInterface>(JsonRepositoryWithBrowser.BrowserPartitionId, "claimByBrowser", e => e.BrowserId, e => e.Id);
//            tableNameProv.Add<ClaimInterface>(JsonRepositoryWithBrowser.AggregateIdPartition, "claimByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<CommentInterface>("comment", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<CommentInterface>(JsonRepository.AggregateIdPartition, "commentByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<FlierBehaviourInterface>("flierbehaviour", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<FlierBehaviourInterface>(JsonRepository.FriendlyIdPartiton, "flierbehaviourFriendly", e => e.FriendlyId, e => e.Id);

            tableNameProv.Add<PaymentTransaction>("paymentTransaction", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<PaymentTransaction>(JsonRepository.AggregateIdPartition, "paymentTransactionByAggregate", e => e.AggregateId, e => e.Id.ToDescendingTimeKey(e.Time));

            tableNameProv.Add<CreditTransaction>("creditTransaction", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<CreditTransaction>(JsonRepository.AggregateIdPartition, "creditTransactionByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<TinyUrlRecord>("tinyurlentity", e => e.AggregateId, e => e.Id);
//            tableNameProv.Add<TinyUrlRecordInterface>(JsonRepository.AggregateIdPartition, "tinyurlentityByAggregate", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<JobBase>("jobs", e => e.Id);

            tableNameProv.Add<FlierAnalyticInterface>("analytics", e => e.Id);
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
