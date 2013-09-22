using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using NUnit.Framework;
using Ninject;
using PostaFlya.DataRepository.DomainQuery.Flyer;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.Environment;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using PostaFlya.Mocks.Domain.Data;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;
using Website.Test.Common;

namespace PostaFlya.DataRepository.Tests
{
    [TestFixture("dev")]
    //[TestFixture("real")]
    public class FlierRepositoryAndSearchServiceTests
    {
//        private GenericRepositoryInterface _repository;
//        private GenericQueryServiceInterface _queryService;
       // private FlierSearchServiceInterface _searchService;
        private QueryChannelInterface _queryChannel;

        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        private string _env;
        public FlierRepositoryAndSearchServiceTests(string env)
        {
            _env = env;
            AzureEnv.UseRealStorage = env == "real";
        }


        private Suburb _loc = new Suburb()
            {
                Locality = "Brunswick",
                RegionCode = "VIC",
                Region = "Victoria",
                PostCode = "3057",
                CountryCode = "AU",
                CountryName = "Australia",
                Longitude = 55, Latitude = 55,
            };
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {

            //Kernel.Get<SqlSeachDbInitializer>().Initialize();

            
//            _repository = Kernel.Get<GenericRepositoryInterface>();
//            _queryService = Kernel.Get<GenericQueryServiceInterface>();
            _queryChannel = Kernel.Get<QueryChannelInterface>();



            //_searchService = Kernel.Get<FlierSearchServiceInterface>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            //Kernel.Unbind<FlierBehaviourInterface>();
            //Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        [SetUp]
        public void BeforeTestDeleteAll()
        {
            DeleteAll();
            _loc.Id = _loc.FriendlyId = _loc.GetGeneratedId();
            if (StoreGetUpdate.Get<Suburb>(_loc.Id, Kernel) == null)
                StoreGetUpdate.Store(_loc, Kernel);
        }

        private void DeleteAll()
        {
            var tableNameProv = Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            var tctx = Kernel.Get<TableContextInterface>();
            foreach (var tableName in tableNameProv.GetAllTableNames())
            {
                tctx.Delete<JsonTableEntry>(tableName, null);
            }
            tctx.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);
//            var context = Kernel.Get<AzureTableContext>("flier");
//            context.Delete<FlierTableEntry>(null, FlierStorageDomain.IdPartition);
//            context.Delete<FlierTableEntry>(null, FlierStorageDomain.BrowserPartition);
//            Kernel.Get<SqlSeachDbInitializer>().DeleteAll();

//            context.Delete<FlierSearchEntry>(null, FlierStorageDomain.CreatedDateSearchPartition);
//            context.Delete<FlierSearchEntry>(null, FlierStorageDomain.EffectiveDateSearchPartition);
//            context.Delete<FlierSearchEntry>(null, FlierStorageDomain.PopularitySearchPartition);
//            context.SaveChanges();
        }

        [Test]
        public void TestCreateFlierRepository()
        {
            using (Kernel.Get<UnitOfWorkInterface>().Begin())
            {
                var repository = Kernel.Get<GenericRepositoryInterface>();
                Assert.IsNotNull(repository);
                Assert.That(repository, Is.InstanceOf<JsonRepository>());

                var queryService = Kernel.Get<GenericQueryServiceInterface>();
                Assert.IsNotNull(queryService);
                Assert.That(queryService, Is.InstanceOf<JsonRepository>());
            }
        }

        

        [Test]
        public void StoreFlierRepositoryTest()
        {
            StoreFlierRepository();
        }

        public Domain.Flier.Flier StoreFlierRepository()
        {
            var eventDateOne = new DateTimeOffset(new DateTime(2076, 8, 11), TimeSpan.FromHours(10));
            var eventDateTwo = new DateTimeOffset(new DateTime(2077, 12, 19), TimeSpan.FromHours(10));


            var flier = FlierTestData.GetOne(Kernel, _loc);
            var board = BoardTestData.GetOne(Kernel, "TestBoard", BoardTypeEnum.VenueBoard, _loc);


            var uow = Kernel.Get<UnitOfWorkInterface>().Begin();

            var qs = Kernel.Get<GenericQueryServiceInterface>();
            var qc = Kernel.Get<QueryChannelInterface>();
            var rand = new Random();

            using (uow)
            {
//                var beh = FlierTestData.GetBehaviour(Kernel, flier);
                flier.EventDates = new List<DateTimeOffset>(){eventDateOne, eventDateTwo};
                StoreGetUpdate.Store(board, Kernel);
                flier.AddBoard(board);
                StoreGetUpdate.Store(flier, Kernel);
                

                var earlierFlier = new Domain.Flier.Flier();
                earlierFlier.CopyFieldsFrom(flier);
                earlierFlier.CreateDate = earlierFlier.CreateDate.AddDays(-1);
                earlierFlier.Id = Guid.NewGuid().ToString();
                earlierFlier.FriendlyId = qc.FindFreeFriendlyIdForFlier(earlierFlier);
                earlierFlier.EventDates = new List<DateTimeOffset>() { eventDateTwo };
                earlierFlier.AddBoard(board);
                StoreGetUpdate.Store(earlierFlier, Kernel);

                var flierCreatedSameDay = new Domain.Flier.Flier();
                flierCreatedSameDay.CopyFieldsFrom(flier);
                flierCreatedSameDay.CreateDate = earlierFlier.CreateDate.AddSeconds(-1);
                flierCreatedSameDay.Id = Guid.NewGuid().ToString();
                flierCreatedSameDay.FriendlyId = qc.FindFreeFriendlyIdForFlier(flierCreatedSameDay);
                flierCreatedSameDay.AddBoard(board);
                StoreGetUpdate.Store(flierCreatedSameDay, Kernel);
                

                //add fliers with variations on longitude and latitude
                var outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.CreateDate = outOfRangeFlier.CreateDate.AddSeconds(rand.Next(-1000, 1000));           
                outOfRangeFlier.FriendlyId = qc.FindFreeFriendlyIdForFlier(outOfRangeFlier);
                outOfRangeFlier.Boards.Clear();
                var outOfRangeBoard = BoardTestData.GetOne(Kernel, "OutBoard", BoardTypeEnum.VenueBoard, new Location(board.Venue().Address.Longitude + 10, board.Venue().Address.Latitude));
                StoreGetUpdate.Store(outOfRangeBoard, Kernel);
                outOfRangeFlier.AddBoard(outOfRangeBoard);
                StoreGetUpdate.Store(outOfRangeFlier, Kernel);


                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.CreateDate = outOfRangeFlier.CreateDate.AddSeconds(rand.Next(-1000, 1000));
                outOfRangeFlier.FriendlyId = qc.FindFreeFriendlyIdForFlier(outOfRangeFlier);
                outOfRangeFlier.Boards.Clear();
                outOfRangeBoard = BoardTestData.GetOne(Kernel, "OutBoard2", BoardTypeEnum.VenueBoard, new Location(board.Venue().Address.Longitude , board.Venue().Address.Latitude + 10));
                StoreGetUpdate.Store(outOfRangeBoard, Kernel);
                outOfRangeFlier.AddBoard(outOfRangeBoard);
                StoreGetUpdate.Store(outOfRangeFlier, Kernel);


                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.CreateDate = outOfRangeFlier.CreateDate.AddSeconds(rand.Next(-1000, 1000));
                outOfRangeFlier.FriendlyId = qc.FindFreeFriendlyIdForFlier(outOfRangeFlier);
                outOfRangeFlier.Boards.Clear();
                outOfRangeBoard = BoardTestData.GetOne(Kernel, "OutBoard3", BoardTypeEnum.VenueBoard, new Location(board.Venue().Address.Longitude + 10, board.Venue().Address.Latitude + 10));
                StoreGetUpdate.Store(outOfRangeBoard, Kernel);
                outOfRangeFlier.AddBoard(outOfRangeBoard);
                StoreGetUpdate.Store(outOfRangeFlier, Kernel);

            }

            Assert.IsTrue(uow.Successful);

            return flier;
        }

        [Test]
        public void GetByIdFlierRepositoryTest()
        {
            GetByIdFlierRepository();
        }

        public FlierInterface GetByIdFlierRepository()
        {
            var storedFlier = StoreFlierRepository();
            FlierTestData.AssertStoreRetrieve(storedFlier, StoreGetUpdate.Get<Flier>(storedFlier.Id, Kernel));
            return storedFlier;
        }

        [Test]
        public void TestIterateAllFliers()
        {
            var storedFlier = StoreFlierRepository();

            int count = 0;
            string skip = null;
            do
            {
                var ret = StoreGetUpdate.GetAll<Flier>(skip, Kernel, 1);
                skip = ret.LastOrDefault();
                count += ret.Count();

            } while (skip != null);


            Assert.That(count, Is.EqualTo(6));

        }


        [Test]
        public void FindFliersByLocationAndDistanceAndDateTest()
        {
            var storedFlier = StoredFlier();

            var location = _loc;
            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));
            var eventDateOne = new DateTime(2076, 8, 11);
            var eventDateTwo = new DateTime(2077, 12, 19);
            var eventDateThree = new DateTime(2087, 12, 19);

            using (Kernel.Get<UnitOfWorkInterface>().Begin())
            {

                var retrievedFliers = _queryChannel.Query(new FindFlyersByDateAndLocationQuery()
                    {
                        Location = location,
                        Distance = 5,
                        Start = eventDateOne,
                        End = eventDateOne + TimeSpan.FromDays(100)
                    }, new List<Flier>());


                //Assert.That(retrievedFliers.Count(), Is.EqualTo(3));
                Assert.That(retrievedFliers.All(_ => _.EventDates.Any(time => time >= eventDateOne)), Is.True);
                AssertRetrievedFliersAreSameLocation(retrievedFliers.AsQueryable(), Kernel.Get<QueryChannelInterface>());

                retrievedFliers = _queryChannel.Query(new FindFlyersByDateAndLocationQuery()
                    {
                        Location = location,
                        Distance = 5,
                        Start = eventDateTwo,
                        End = eventDateTwo + TimeSpan.FromDays(100)
                    }, new List<Flier>());


                //Assert.That(retrievedFliers.Count(), Is.EqualTo(3));
                AssertUtil.AreAll(retrievedFliers, flier => flier.EventDates.Any(time => time.DateTime >= eventDateTwo));
                AssertRetrievedFliersAreSameLocation(retrievedFliers.AsQueryable(), Kernel.Get<QueryChannelInterface>());

                retrievedFliers = _queryChannel.Query(new FindFlyersByDateAndLocationQuery()
                    {
                        Location = location,
                        Distance = 5,
                        Start = eventDateThree,
                        End = eventDateThree + TimeSpan.FromDays(100)
                    }, new List<Flier>());

                Assert.That(retrievedFliers.Count(), Is.EqualTo(0));
            }
        }

        private Flier StoredFlier()
        {
            var storedFlier = StoreFlierRepository();
            return storedFlier;
        }

        [Test]
        public void FindNearByBoardsFindsBoardsWithinXMeters()
        {
            var board = BoardTestData.GetOne(Kernel, "TestBoardNameNoLoc", BoardTypeEnum.VenueBoard, _loc);
            StoreGetUpdate.Store(board, Kernel);

            var loc2 = new Location(_loc);
            loc2.Latitude += 1;
            loc2.Longitude += 1;
            var board2 = BoardTestData.GetOne(Kernel, "TestBoardNameNoLoc", BoardTypeEnum.VenueBoard, loc2);
            StoreGetUpdate.Store(board2, Kernel);

            var nearbyQuery = Kernel.Get<FindBoardsNearQueryHandler>();
            var nearby = nearbyQuery.Query(new FindBoardsNearQuery() {Location = _loc, WithinMetres = 1});

            Assert.That(nearby, Is.Not.Null);
            Assert.That(nearby.Count, Is.EqualTo(1));

            Assert.That(nearby[0], Is.EqualTo(board.Id));
        }

        [Test]
        public void FindFliersByBoard()
        {
            var storedFlier = StoreFlierRepository();

            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            using (Kernel.Get<UnitOfWorkInterface>().Begin())
            {
                var retrievedFliers = _queryChannel.Query(new FindFlyersByBoardQuery
                    {
                        BoardId = storedFlier.Boards.First().BoardId,
                        Start = DateTimeOffset.MinValue,
                        End = DateTimeOffset.MaxValue.AddDays(-1)
                    }, new List<Flier>());

                Assert.That(retrievedFliers.Count(), Is.EqualTo(3));
            }

        }

        [Test]
        public void FindFliersByBoardAndDateTest()
        {
            var flier = StoredFlier();

            var eventDateOne = new DateTime(2076, 8, 11);
            var eventDateTwo = new DateTime(2087, 12, 19);;

            var board = flier.Boards.FirstOrDefault();

            using (Kernel.Get<UnitOfWorkInterface>().Begin())
            {
                var retrievedFliers = _queryChannel.Query(new FindFlyersByBoardQuery
                {
                    BoardId = board.BoardId,
                    Start = eventDateOne,
                    End = eventDateOne.AddDays(100)
                }, new List<Flier>());

                Assert.That(retrievedFliers.All(f => f.EventDates.Any(time => time >= eventDateOne)), Is.True);

                retrievedFliers = _queryChannel.Query(new FindFlyersByBoardQuery
                {
                    BoardId = board.BoardId,
                    Start = eventDateTwo,
                    End = eventDateTwo.AddDays(100)
                }, new List<Flier>());

                Assert.That(retrievedFliers.Count(), Is.EqualTo(0));
            }

        }



        [Test]
        public void FlierRepositoryGetByBrowserIdTest()
        {
            FlierRepositoryGetByBrowserId();
        }

        public IQueryable<FlierInterface> FlierRepositoryGetByBrowserId()
        {
            var storedFlier = StoreFlierRepository();

            using (Kernel.Get<UnitOfWorkInterface>().Begin())
            {
                var qc = Kernel.Get<QueryChannelInterface>();
                var retrievedFlier = qc.Query(new GetByBrowserIdQuery<Flier>() { BrowserId = storedFlier.BrowserId },
                                              new List<Flier>());

                Assert.IsTrue(retrievedFlier.Any());
                var retrieved = retrievedFlier.SingleOrDefault(f => f.Id == storedFlier.Id);
                FlierTestData.AssertStoreRetrieve(storedFlier, retrieved);

                return retrievedFlier.AsQueryable();
            }

        }

        [Test]
        public void AzureFlierRepositoryCommentUpdatesNumberOfComments()
        {
            var board = BoardTestData.GetAndStoreOne(Kernel);
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier.AddBoard(board);
            StoreGetUpdate.Store(testFlier, Kernel);

            var retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);
            FlierTestData.AssertStoreRetrieve(testFlier, retFlier);

            var addComment = CommentTestData.GetOne(Kernel, retFlier.Id);

            StoreGetUpdate.Store(addComment, Kernel);
            testFlier.NumberOfComments++;
            StoreGetUpdate.UpdateOne<Flier>(testFlier, Kernel, FlierInterfaceExtensions.CopyFieldsFrom);

            retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);
            Assert.AreEqual(1, retFlier.NumberOfComments);

        }


        [Test]
        public void AzureFlierRepositoryGetCommentsReturnsAllCommentsOnAFlier()
        {
            var board = BoardTestData.GetAndStoreOne(Kernel);
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier.AddBoard(board);
            StoreGetUpdate.Store(testFlier, Kernel);
            var retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);


            var comments = CommentTestData.GetSome(Kernel, retFlier.Id, 5);
            foreach (var comment in comments)
            {
                StoreGetUpdate.Store(comment,  Kernel);
                testFlier.NumberOfComments++;
            }
            StoreGetUpdate.UpdateOne<Flier>(testFlier, Kernel, FlierInterfaceExtensions.CopyFieldsFrom);


            var retComments = StoreGetUpdate.GetByAggRoot<Comment>(retFlier.Id, Kernel);
            AssertUtil.Count(5, retComments);
            //comments in order
            CollectionAssert.AreEqual(comments, retComments, new CommentTestData.CommentTestDataEq());

            retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);
            Assert.AreEqual(5, retFlier.NumberOfComments);

        }

        [Test]
        public void AzureFlierRepositoryClaimUpdatesNumberOfClaims()
        {
            var board = BoardTestData.GetAndStoreOne(Kernel);
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier.AddBoard(board);
            StoreGetUpdate.Store(testFlier, Kernel);
            var retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);

            var claim = ClaimTestData.GetOne(Kernel, retFlier.Id);
            StoreGetUpdate.Store(claim, Kernel);
            testFlier.NumberOfClaims++;
            StoreGetUpdate.UpdateOne<Flier>(testFlier, Kernel, FlierInterfaceExtensions.CopyFieldsFrom);

            retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);
            Assert.AreEqual(1, retFlier.NumberOfClaims);

        }


        [Test]
        public void AzureFlierRepositoryGetClaimsReturnsAllClaimsOnAFlier()
        {
            var board = BoardTestData.GetAndStoreOne(Kernel);
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier.AddBoard(board);
            StoreGetUpdate.Store(testFlier, Kernel);
            var retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);

            var claims = ClaimTestData.GetSome(Kernel, retFlier.Id, 5);
            foreach (var claim in claims)
            {
                StoreGetUpdate.Store(claim, Kernel);
                testFlier.NumberOfClaims++;
            }
            StoreGetUpdate.UpdateOne<Flier>(testFlier, Kernel, FlierInterfaceExtensions.CopyFieldsFrom);

            var retClaims =StoreGetUpdate.GetByAggRoot<Claim>(retFlier.Id, Kernel);
            AssertUtil.Count(5, retClaims);
            retClaims = retClaims.OrderBy(claim => claim.ClaimTime);
            //the first claims should be stored first
            CollectionAssert.AreEqual(claims, retClaims, new ClaimTestData.ClaimTestDataEq());

            retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);
            Assert.AreEqual(5, retFlier.NumberOfClaims);

        }

        [Test]
        public void AzureFlierRepositoryGetEntitiesClaimedByBrowserReturnsAllFlierClaimed()
        {

            var board = BoardTestData.GetAndStoreOne(Kernel);

            var claims = new List<ClaimInterface>();
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier.AddBoard(board);
            StoreGetUpdate.Store(testFlier, Kernel);
            var retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);
            var claim = ClaimTestData.GetOne(Kernel, retFlier.Id);
            var browserId = claim.BrowserId;
            ClaimTestData.ClaimOne(testFlier, claim, Kernel);
            claims.Add(claim);

            testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier.AddBoard(board);
            StoreGetUpdate.Store(testFlier, Kernel);
            retFlier = StoreGetUpdate.Get<Flier>(testFlier.Id, Kernel);
            claim = ClaimTestData.GetOne(Kernel, retFlier.Id);
            claim.BrowserId = browserId;
            ClaimTestData.ClaimOne(testFlier, claim, Kernel);
            claims.Add(claim);

            using (Kernel.Get<UnitOfWorkInterface>().Begin())
            {
                var qc = Kernel.Get<QueryChannelInterface>();
                var retClaims = qc.Query(new GetByBrowserIdQuery<Claim>() { BrowserId = browserId }, new List<Claim>()).AsQueryable();
                retClaims = retClaims.OrderByDescending(c => c.ClaimTime);
                AssertUtil.Count(2, retClaims);
                //the latest claims should be stored first
                CollectionAssert.AreEqual(claims.AsQueryable().Reverse(), retClaims, new ClaimTestData.ClaimTestDataEq());
            }

        }


        private void AssertRetrievedFliersAreSameLocation(IQueryable<FlierInterface> retrievedFliers, QueryChannelInterface query)
        {
            foreach (var retrievedFlierCombo in from r1 in retrievedFliers
                                                from r2 in retrievedFliers
                                                select new
                                                    {
                                                        r1 = query.Query(new GetFlyerVenueBoardQuery(){FlyerId = r1.Id}, (Board)null), 
                                                        r2 = query.Query(new GetFlyerVenueBoardQuery(){FlyerId = r2.Id}, (Board)null),
                                                    })
            {
                
                Assert.AreEqual(retrievedFlierCombo.r1.Venue().Address, retrievedFlierCombo.r2.Venue().Address);
            }
        }
    }


}
