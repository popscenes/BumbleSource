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
using Website.Domain.Location.Query;
using Website.Domain.Payment;
using Website.Domain.Query;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
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
        public const string GeoSearchIndex = "G";

        public const string FlyerSuburbSearchIndex = "FS";
        public const string FlyerBoardSearchIndex = "FB";

        public const string BoardSuburbSearchIndex = "BS";
        private const int DefaultNearByIndex = 40;

        public static Expression<Func<QueryChannelInterface, EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> BrowserIdSelector<EntityInterfaceType>() where EntityInterfaceType : AggregateRootInterface, BrowserIdInterface
        {
            Expression<Func<QueryChannelInterface, EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory = (qc, root) => new List<StorageTableKeyInterface>()
                {
                    new StorageTableKey()
                        {
                            PartitionKey = root.BrowserId.ToStorageKeySection(),
                            RowKey = root.Id.ToStorageKeySection()
                        }
                };
            return indexEntryFactory;
        }

        public static Expression<Func<QueryChannelInterface, EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> BrowserIdSelectorForAggregate<EntityInterfaceType>() where EntityInterfaceType : AggregateInterface, BrowserIdInterface
        {
            Expression<Func<QueryChannelInterface, EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory = 
                (qc, root) => new List<StorageTableKeyInterface>()
                {
                    new StorageTableKey()
                        {
                            PartitionKey = root.BrowserId.ToStorageKeySection(),
                            RowKey = root.AggregateId.ToStorageKeySection() + root.Id.ToStorageKeySection()
                        }
                };
            return indexEntryFactory;
        }

        public static Expression<Func<QueryChannelInterface, BrowserInterface, IEnumerable<StorageTableKeyInterface>>> BrowserCredentialSelector()
        {
            Expression<Func<QueryChannelInterface, BrowserInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory
                = (qc, browser) =>

                  browser.ExternalCredentials.Select(
                      credential =>
                      new StorageTableKey()
                          {
                              PartitionKey = credential.ToUniqueString().ToStorageKeySection(),
                              RowKey = browser.Id.ToStorageKeySection()
                          });
                    
            return indexEntryFactory;
        }

        public static Expression<Func<QueryChannelInterface, BrowserInterface, IEnumerable<StorageTableKeyInterface>>> BrowserEmailSelector()
        {
            Expression<Func<QueryChannelInterface, BrowserInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory
                = (qc, browser) =>

                  new List<StorageTableKeyInterface>() {new StorageTableKey()
                      {
                          PartitionKey = browser.EmailAddress.ToStorageKeySection(), 
                          RowKey = browser.Id.ToStorageKeySection()
                      }};
                    
            return indexEntryFactory;
        }

        public static Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> BaordAdminEmailSelector()
        {
            Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory
                = (qc, board) =>
                  board.AdminEmailAddresses == null ? new List<StorageTableKey>() { new StorageTableKey() { RowKey = board.Id.ToStorageKeySection() } } : 
                  board.AdminEmailAddresses.Select(adimEmail => new StorageTableKey() { PartitionKey = adimEmail.ToStorageKeySection(), RowKey = board.Id.ToStorageKeySection() });

            return indexEntryFactory;
        }

        public static Expression<Func<QueryChannelInterface, EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>>
            TinyUrlSelector<EntityInterfaceType>() where EntityInterfaceType : EntityWithTinyUrlInterface, AggregateRootInterface
        {
            Expression<Func<QueryChannelInterface, EntityInterfaceType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory = (qc, root) => new List<StorageTableKeyInterface>()
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


        public static string ToGeoLatSearchKey(this double lat)
        {
            return Math.Round(lat + 90, 3).ToString("000.000").ToStorageKeySection();
        }

        public static string ToGeoLongSearchKey(this double @long)
        {
            return Math.Round(@long + 180, 3).ToString("000.000").ToStorageKeySection();
        }

        public static Expression<Func<QueryChannelInterface, SuburbEntityInterface, IEnumerable<StorageTableKeyInterface>>>
            SuburbGeoSearchSelector()
        {

            return
                (qc, suburb) => new List<StorageTableKeyInterface>()
                    {
                        new JsonTableEntry(new GeoCoords()
                            {
                                Latitude = suburb.Latitude,
                                Longitude = suburb.Longitude
                            })
                            {
                                PartitionKey = suburb.Longitude.ToGeoLongSearchKey(),
                                RowKey = suburb.Latitude.ToGeoLatSearchKey() +  suburb.Id.ToStorageKeySection()
                            }
                    };
        }

        public static Expression<Func<QueryChannelInterface, SuburbEntityInterface, IEnumerable<StorageTableKeyInterface>>> SuburbSearchSelector()
        {

            Expression<Func<QueryChannelInterface, SuburbEntityInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory
                =
                (qc, suburb) => 
                    suburb.GetSuburbDescription(true)
                    .ToTermsSearchKeys()
                    .Select((s, i) => new JsonTableEntry(new SearchEntityRecord()
                    {
                        Id = suburb.Id,
                        FriendlyId = suburb.FriendlyId,
                        TypeOfEntity = suburb.GetType().GetAssemblyQualifiedNameWithoutVer(),
                        DisplayString = suburb.GetSuburbDescription(true)
                    })
                    {

                        PartitionKey = s + i.ToRowKeyWithFieldWidth(10).ToStorageKeySection(),
                        RowKey = suburb.Id.ToStorageKeySection()
                    });


            return indexEntryFactory;
        }


        public static Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> BoardSearchSelector()
        {

            Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory
                =
                (qc, board) =>
                    board.Name
                    .ToTermsSearchKeys()
                    .Select((s, i) => new JsonTableEntry(new SearchEntityRecord()
                    {
                        Id = board.Id,
                        FriendlyId = board.FriendlyId,
                        TypeOfEntity = board.GetType().GetAssemblyQualifiedNameWithoutVer(),
                        DisplayString = board.Name
                    })
                    {
                        PartitionKey = s + i.ToRowKeyWithFieldWidth(10).ToStorageKeySection(),
                        RowKey = board.Id.ToStorageKeySection()
                    });


            return indexEntryFactory;
        }

        public static Expression<Func<QueryChannelInterface, FlierInterface, IEnumerable<StorageTableKeyInterface>>>
            FlyerSuburbSelector()
        {
            Expression<Func<QueryChannelInterface, FlierInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory =
                (qc, flyer) => 
                    qc.Query(new FindSuburbsWithinDistanceOfGeoCoordsQuery
                    {
                        Geo = flyer.GetVenueForFlier(qc).Address.AsGeoCoords(),
                        Kilometers = DefaultNearByIndex
                    }, new List<Suburb>())
                    .Select(s => new { s, v = flyer.GetVenueForFlier(qc).Address.AsGeoCoords()})
                    .SelectMany(sv => 
                        flyer.EventDates.Distinct().Select(e =>
                            new JsonTableEntry(sv.v)
                            {
                                PartitionKey = sv.s.Id.ToStorageKeySection() + e.GetTimestampAscending().ToStorageKeySection(),
                                RowKey = flyer.Id.ToStorageKeySection()
                            }));

            return indexEntryFactory;

        }

        public static Expression<Func<QueryChannelInterface, FlierInterface, IEnumerable<StorageTableKeyInterface>>>
            FlyerBoardSelector()
        {
            Expression<Func<QueryChannelInterface, FlierInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory =
                (qc, flyer) => flyer.Boards.Select(bf => bf.BoardId).Distinct().SelectMany(b
                    => flyer.EventDates.Distinct().Select(e =>
                            new StorageTableKey()
                            {
                                PartitionKey = b.ToStorageKeySection() + e.GetTimestampAscending().ToStorageKeySection(),
                                RowKey = flyer.Id.ToStorageKeySection()
                            }
                    ));

            return indexEntryFactory;
        }


        public static Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>>
            BoardSuburbSelector()
        {
            Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory =
                (qc, board) => board.Venue() == null ? new List<JsonTableEntry>() : 
                    
                    qc.Query(new FindSuburbsWithinDistanceOfGeoCoordsQuery
                {
                    Geo = board.Venue().Address.AsGeoCoords(),
                    Kilometers = DefaultNearByIndex
                }, new List<Suburb>())
                    .Select(suburb =>
                            new JsonTableEntry(board.Venue().Address.AsGeoCoords())
                            {
                                PartitionKey = suburb.Id.ToStorageKeySection(),
                                RowKey = board.Id.ToStorageKeySection()
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
            tableNameProv.AddIndex<EntityInterface, BoardInterface>("textSearchIDX", DomainIndexSelectors.TextSearchIndex, DomainIndexSelectors.BoardSearchSelector());
            tableNameProv.AddIndex("boardSuburbIDX", DomainIndexSelectors.BoardSuburbSearchIndex, DomainIndexSelectors.BoardSuburbSelector());


            tableNameProv.Add<FlierInterface>("flierEntity", e => e.Id);
            tableNameProv.AddIndex("flierFriendlyIDX", StandardIndexSelectors.FriendlyIdIndex, StandardIndexSelectors.FriendlyIdSelector<FlierInterface>());
            tableNameProv.AddIndex("flierBrowserIDX", DomainIndexSelectors.BrowserIdIndex, DomainIndexSelectors.BrowserIdSelector<FlierInterface>());
            tableNameProv.AddIndex("flyerSuburbIDX", DomainIndexSelectors.FlyerSuburbSearchIndex, DomainIndexSelectors.FlyerSuburbSelector());
            tableNameProv.AddIndex("flyerBoardIDX", DomainIndexSelectors.FlyerBoardSearchIndex, DomainIndexSelectors.FlyerBoardSelector());
            
            tableNameProv.Add<BrowserInterface>("browserEntity", e => e.Id);
            tableNameProv.AddIndex("browserFriendlyIDX", StandardIndexSelectors.FriendlyIdIndex, StandardIndexSelectors.FriendlyIdSelector<BrowserInterface>());
            tableNameProv.AddIndex("browserCredIDX", DomainIndexSelectors.BrowserCredentialIndex, DomainIndexSelectors.BrowserCredentialSelector());
            tableNameProv.AddIndex("browserEmailIDX", DomainIndexSelectors.BrowserEmailIndex, DomainIndexSelectors.BrowserEmailSelector());

            tableNameProv.Add<ImageInterface>("imageEntity", e => e.Id);
            tableNameProv.AddIndex("imageBrowserIDX", DomainIndexSelectors.BrowserIdIndex, DomainIndexSelectors.BrowserIdSelector<ImageInterface>());

            tableNameProv.Add<ClaimInterface>("claimEntity", e => e.AggregateId, e => e.Id);
            tableNameProv.AddIndex("claimBrowserIDX", DomainIndexSelectors.BrowserIdIndex, DomainIndexSelectors.BrowserIdSelectorForAggregate<ClaimInterface>());

            tableNameProv.Add<CommentInterface>("commentEntity", e => e.AggregateId, e => e.Id);


            tableNameProv.Add<PaymentTransaction>("paymentTransactionEntity", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<CreditTransaction>("creditTransactionEntity", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<TinyUrlRecord>("tinyurlentityEntity", e => e.AggregateId, e => e.Id);

            tableNameProv.Add<JobBase>("jobsEntity", e => e.Id);

            tableNameProv.Add<FlierAnalyticInterface>("analyticsEntity", e => e.Id);

            tableNameProv.Add<SuburbEntityInterface>("suburbEntity", e => e.Id);
            tableNameProv.AddIndex<EntityInterface, SuburbEntityInterface>("textSearchIDX", DomainIndexSelectors.TextSearchIndex, DomainIndexSelectors.SuburbSearchSelector());
            tableNameProv.AddIndex("geoSearchIDX", DomainIndexSelectors.GeoSearchIndex, DomainIndexSelectors.SuburbGeoSearchSelector());


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
