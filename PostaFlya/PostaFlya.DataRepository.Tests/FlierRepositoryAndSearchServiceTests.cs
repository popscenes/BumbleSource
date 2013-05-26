using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using PostaFlya.DataRepository.Internal;
using PostaFlya.DataRepository.Search.Event;
using PostaFlya.DataRepository.Search.Query;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Flier.Event;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.Environment;
using PostaFlya.DataRepository.Search.Implementation;
using PostaFlya.DataRepository.Tests.Internal;
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
        private GenericRepositoryInterface _repository;
        private QueryServiceForBrowserAggregateInterface _queryService;
        private FlierSearchServiceInterface _searchService;

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


        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            new AzureCommentRepositoryTests(_env).FixtureSetUp();
            new AzureClaimRepositoryTests(_env).FixtureSetUp();

            Kernel.Get<SqlSeachDbInitializer>().Initialize();
            DeleteAll();

            
            _repository = Kernel.Get<GenericRepositoryInterface>();
            _queryService = Kernel.Get<QueryServiceForBrowserAggregateInterface>();
            _searchService = Kernel.Get<FlierSearchServiceInterface>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<FlierBehaviourInterface>();
            //Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        private void DeleteAll()
        {
//            var context = Kernel.Get<AzureTableContext>("flier");
//            context.Delete<FlierTableEntry>(null, FlierStorageDomain.IdPartition);
//            context.Delete<FlierTableEntry>(null, FlierStorageDomain.BrowserPartition);
            Kernel.Get<SqlSeachDbInitializer>().DeleteAll();

//            context.Delete<FlierSearchEntry>(null, FlierStorageDomain.CreatedDateSearchPartition);
//            context.Delete<FlierSearchEntry>(null, FlierStorageDomain.EffectiveDateSearchPartition);
//            context.Delete<FlierSearchEntry>(null, FlierStorageDomain.PopularitySearchPartition);
//            context.SaveChanges();
        }

        [Test]
        public void TestCreateFlierRepository()
        {
            var repository = Kernel.Get<GenericRepositoryInterface>();
            Assert.IsNotNull(repository);
            Assert.That(repository, Is.InstanceOf<JsonRepository>());

            var queryService = Kernel.Get<QueryServiceForBrowserAggregateInterface>();
            Assert.IsNotNull(queryService);
            Assert.That(queryService, Is.InstanceOf<JsonRepositoryWithBrowser>());
        }

        private static Location _loc = new Location(55,55);

        [Test]
        public void StoreFlierRepositoryTest()
        {
            StoreFlierRepository();
        }

        public Domain.Flier.Flier StoreFlierRepository()
        {
            DeleteAll();

            var eventDateOne = new DateTimeOffset(new DateTime(2076, 8, 11), TimeSpan.FromHours(10));
            var eventDateTwo = new DateTimeOffset(new DateTime(2077, 12, 19), TimeSpan.FromHours(10));


            var flier = FlierTestData.GetOne(Kernel, _loc);
            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {_repository});

            var qs = Kernel.Get<GenericQueryServiceInterface>();
            var rand = new Random();
            var indexer = Kernel.Get<SqlFlierIndexService>();
            using (uow)
            {
                var beh = FlierTestData.GetBehaviour(Kernel, flier);
                flier.EventDates = new List<DateTimeOffset>(){eventDateOne, eventDateTwo};
                _repository.Store(flier);
                _repository.SaveChanges();
                indexer.Handle(new FlierModifiedEvent(){NewState = flier});

                var earlierFlier = new Domain.Flier.Flier();
                earlierFlier.CopyFieldsFrom(flier);
                earlierFlier.CreateDate = earlierFlier.CreateDate.AddDays(-1);
                earlierFlier.Id = Guid.NewGuid().ToString();
                earlierFlier.FriendlyId = qs.FindFreeFriendlyIdForFlier(earlierFlier);
                earlierFlier.EventDates = new List<DateTimeOffset>() { eventDateTwo };
                _repository.Store(earlierFlier);
                _repository.SaveChanges();
                indexer.Handle(new FlierModifiedEvent() { NewState = earlierFlier });

                var flierCreatedSameDay = new Domain.Flier.Flier();
                flierCreatedSameDay.CopyFieldsFrom(flier);
                flierCreatedSameDay.CreateDate = earlierFlier.CreateDate.AddSeconds(-1);
                flierCreatedSameDay.Id = Guid.NewGuid().ToString();
                flierCreatedSameDay.FriendlyId = qs.FindFreeFriendlyIdForFlier(flierCreatedSameDay);
                _repository.Store(flierCreatedSameDay);
                _repository.SaveChanges();
                indexer.Handle(new FlierModifiedEvent() { NewState = earlierFlier });
                

                //add fliers with variations on longitude and latitude
                var outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.CreateDate = outOfRangeFlier.CreateDate.AddSeconds(rand.Next(-1000, 1000));
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude);
                outOfRangeFlier.FriendlyId = qs.FindFreeFriendlyIdForFlier(outOfRangeFlier);
                _repository.Store(outOfRangeFlier);
                _repository.SaveChanges();
                indexer.Handle(new FlierModifiedEvent() { NewState = outOfRangeFlier });

                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.CreateDate = outOfRangeFlier.CreateDate.AddSeconds(rand.Next(-1000, 1000));
                outOfRangeFlier.Location = new Location(flier.Location.Longitude, flier.Location.Latitude + 10);
                outOfRangeFlier.FriendlyId = qs.FindFreeFriendlyIdForFlier(outOfRangeFlier);
                _repository.Store(outOfRangeFlier);
                _repository.SaveChanges();
                indexer.Handle(new FlierModifiedEvent() { NewState = outOfRangeFlier });


                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.CreateDate = outOfRangeFlier.CreateDate.AddSeconds(rand.Next(-1000, 1000));
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude + 10);
                outOfRangeFlier.FriendlyId = qs.FindFreeFriendlyIdForFlier(outOfRangeFlier);
                _repository.Store(outOfRangeFlier);
                _repository.SaveChanges();
                indexer.Handle(new FlierModifiedEvent() { NewState = outOfRangeFlier });

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
            return FlierTestData.AssertGetById(storedFlier, _queryService);
        }

        [Test]
        public void FindFliersByLocationAndTagsRepositoryTest()
        {
            FindFliersByLocationAndTagsRepository();
        }

        [Test]
        public void TestIterateAllFliers()
        {
            var storedFlier = StoreFlierRepository();

            int count = 0;
            FlierInterface skip = null;
            do
            {
                var ret = _searchService.IterateAllIndexedFliers(1, skip);
                skip = ret.LastOrDefault() == null ? null : _queryService.FindById<Domain.Flier.Flier>(ret.LastOrDefault().Id);
                count += ret.Count;

            } while (skip != null);


            Assert.That(count, Is.EqualTo(5));

        }


        [Test]
        public void FindFliersByLocationAndDistanceTest()
        {
            var storedFlier = StoredFlier();

            var location = _loc;
            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            var retrievedFliers = _searchService.FindFliersByLocationAndDistance(location, 5, 30, skipPast: null, tags: tag)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(retrievedFliers.Count(), Is.EqualTo(2));

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFliers.FirstOrDefault());
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

            var retrievedFliers = _searchService.FindFliersByLocationAndDistance(location, 5, 30, null, tag, eventDateOne)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(retrievedFliers.Count(), Is.EqualTo(2));
            Assert.That(retrievedFliers.First().EventDates.Any(time => time == eventDateOne), Is.True);

            retrievedFliers = _searchService.FindFliersByLocationAndDistance(location, 5, 30, null, tag, eventDateTwo)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(retrievedFliers.Count(), Is.EqualTo(2));
            AssertUtil.AreAll(retrievedFliers, flier => flier.EventDates.Any(time => time.DateTime == eventDateTwo));

            retrievedFliers = _searchService.FindFliersByLocationAndDistance(location, 5, 30, null, tag, eventDateThree)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();
            Assert.That(retrievedFliers.Count(), Is.EqualTo(0));
        }

        [Test]
        public void FindFliersByLocationAndDistanceAndDateSKipOrderTest()
        {
            var storedFlier = StoredFlier();

            var location = _loc;
            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));
            var eventDateOne = new DateTime(2076, 8, 11);

            var retrievedFliers = _searchService.FindFliersByLocationAndDistance(location, 5, 1, null, tag, eventDateOne)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(retrievedFliers.Count(), Is.EqualTo(1));
            Assert.That(retrievedFliers.First().EventDates.Any(time => time == eventDateOne), Is.True);

            var moreFliers = _searchService.FindFliersByLocationAndDistance(location, 5, 1, retrievedFliers.FirstOrDefault(), tag, eventDateOne)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(moreFliers.Count(), Is.EqualTo(1));
            Assert.That(moreFliers.First().EventDates.Any(time => time == eventDateOne), Is.True);


        }


        private Flier StoredFlier()
        {
            var storedFlier = StoreFlierRepository();
            var board = BoardTestData.GetOne(Kernel, "TestBoardName", BoardTypeEnum.VenueBoard, _loc);
            board = BoardTestData.StoreOne(board, _repository, Kernel);

            var boardFlier = new BoardFlier()
                {
                    Id = storedFlier.Id + board.Id,
                    AggregateId = board.Id,
                    FlierId = storedFlier.Id,
                    Status = BoardFlierStatus.Approved,
                    DateAdded = DateTime.Now
                };

            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                            .GetUnitOfWork(new List<RepositoryInterface>() {_repository});

            var indexer = Kernel.Get<SqlFlierIndexService>();
            using (uow)
            {
                _repository.Store(boardFlier);
                _repository.UpdateEntity<Domain.Flier.Flier>(storedFlier.Id,
                                                             flier => flier.Boards.Add(board.Id));
            }
            Assert.IsTrue(uow.Successful);

            indexer.Handle(new FlierModifiedEvent()
                {
                    NewState = _queryService.FindById<Domain.Flier.Flier>(storedFlier.Id),
                    OrigState = storedFlier
                });
            indexer.Handle(new BoardFlierModifiedEvent() {NewState = boardFlier});

            return _queryService.FindById<Flier>(storedFlier.Id);
        }

        [Test]
        public void FindNearByBoardsFindsBoardsWithinXMeters()
        {
            var board = BoardTestData.GetOne(Kernel, "TestBoardNameNoLoc", BoardTypeEnum.VenueBoard, _loc);
            board = BoardTestData.StoreOne(board, _repository, Kernel);
            var indexer = Kernel.Get<SqlFlierIndexService>();
            indexer.Handle(new BoardModifiedEvent() { NewState = board });

            var loc2 = new Location(_loc);
            loc2.Latitude += 1;
            loc2.Longitude += 1;
            var board2 = BoardTestData.GetOne(Kernel, "TestBoardNameNoLoc", BoardTypeEnum.VenueBoard, loc2);
            board2 = BoardTestData.StoreOne(board2, _repository, Kernel);
            indexer.Handle(new BoardModifiedEvent() { NewState = board2 });

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
            var board = BoardTestData.GetOne(Kernel, "TestBoardNameNoLoc",BoardTypeEnum.VenueBoard, _loc);
            board = BoardTestData.StoreOne(board, _repository, Kernel);

            var boardFlier = new BoardFlier()
            {
                Id = storedFlier.Id + board.Id,
                AggregateId = board.Id,
                FlierId = storedFlier.Id,
                Status = BoardFlierStatus.Approved,
                DateAdded = DateTime.UtcNow
            };

            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository });

            var indexer = Kernel.Get<SqlFlierIndexService>();
            using (uow)
            {
                _repository.Store(boardFlier);
                indexer.Handle(new BoardFlierModifiedEvent() { NewState = boardFlier });
            }

            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            var retrievedFliers = _searchService.FindFliersByBoard(board.Id, 5, null, null, tag)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(retrievedFliers.Count(), Is.EqualTo(1));

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFliers.FirstOrDefault());
        }

        [Test]
        public void FindFliersByBoardAndDateTest()
        {
            var flier = StoredFlier();

            var eventDateOne = new DateTime(2076, 8, 11);
            var eventDateTwo = new DateTime(2087, 12, 19);;

            var board = flier.Boards.FirstOrDefault();

            var retrievedFliers = _searchService.FindFliersByBoard(board, 30, null, eventDateOne)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(retrievedFliers.Count(), Is.EqualTo(1));
            Assert.That(retrievedFliers.Single().EventDates.Any(time => time == eventDateOne), Is.True);

            retrievedFliers = _searchService.FindFliersByBoard(board, 30, null, eventDateTwo)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(retrievedFliers.Count(), Is.EqualTo(0));

        }

        public IQueryable<FlierInterface> FindFliersByLocationAndTagsRepository()
        {
            var storedFlier = StoreFlierRepository();

            var location = _loc;
            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            var retrievedFliers = _searchService.FindFliersByLocationAndDistance(location, 5, 30, skipPast: null, tags: tag)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).AsQueryable();

            Assert.IsTrue(retrievedFliers.Any());
            AssertRetrievedFliersAreSameLocation(retrievedFliers);


            retrievedFliers = _searchService.FindFliersByLocationAndDistance(new Location(130, 130), 5, 30,  skipPast: null, tags: tag)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).AsQueryable();
            Assert.IsTrue(!retrievedFliers.Any());

            var theBadTags = new Tags(){"crapolla","shitolla"};
            retrievedFliers = _searchService.FindFliersByLocationAndDistance(location, 5, 30, skipPast: null, tags: theBadTags)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).AsQueryable();
            Assert.IsTrue(!retrievedFliers.Any());

            return retrievedFliers;
        }

        [Test]
        public void FlierRepositoryGetByBrowserIdTest()
        {
            FlierRepositoryGetByBrowserId();
        }

        public IQueryable<FlierInterface> FlierRepositoryGetByBrowserId()
        {
            var storedFlier = StoreFlierRepository();
            var retrievedFlier = _queryService.GetByBrowserId<Domain.Flier.Flier>(storedFlier.BrowserId);

            Assert.IsTrue(retrievedFlier.Any());
            var retrieved = retrievedFlier.SingleOrDefault(f => f.Id == storedFlier.Id);
            FlierTestData.AssertStoreRetrieve(storedFlier, retrieved);
            
            return retrievedFlier;
        }

        [Test]
        public void AzureFlierRepositoryCommentUpdatesNumberOfComments()
        {
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);

            var addComment = CommentTestData.GetOne(Kernel, retFlier.Id);

            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository }))
            {

                _repository.Store(addComment);
                testFlier.NumberOfComments++;
            }
            FlierTestData.UpdateOne(testFlier, _repository, Kernel);

            retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            Assert.AreEqual(1, retFlier.NumberOfComments);

        }


        [Test]
        public void AzureFlierRepositoryGetCommentsReturnsAllCommentsOnAFlier()
        {
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);


            var comments = CommentTestData.GetSome(Kernel, retFlier.Id, 5);
            foreach (var comment in comments)
            {
                using (Kernel.Get<UnitOfWorkFactoryInterface>()
                    .GetUnitOfWork(new List<RepositoryInterface>() {_repository}))
                {

                    _repository.Store(comment);
                    testFlier.NumberOfComments++;
                }
            }
            FlierTestData.UpdateOne(testFlier, _repository, Kernel);

            var retComments = _queryService.FindAggregateEntities<Comment>(retFlier.Id);
            AssertUtil.Count(5, retComments);
            //comments in order
            CollectionAssert.AreEqual(comments, retComments, new CommentTestData.CommentTestDataEq());

            retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            Assert.AreEqual(5, retFlier.NumberOfComments);

        }

        [Test]
        public void AzureFlierRepositoryClaimUpdatesNumberOfClaims()
        {
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);

            var claim = ClaimTestData.GetOne(Kernel, retFlier.Id);

            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository }))
            {

                _repository.Store(claim);
                testFlier.NumberOfClaims++;
            }
            FlierTestData.UpdateOne(testFlier, _repository, Kernel);

            retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            Assert.AreEqual(1, retFlier.NumberOfClaims);

        }


        [Test]
        public void AzureFlierRepositoryGetClaimsReturnsAllClaimsOnAFlier()
        {
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);


            var claims = ClaimTestData.GetSome(Kernel, retFlier.Id, 5);
            foreach (var comment in claims)
            {
                using (Kernel.Get<UnitOfWorkFactoryInterface>()
                    .GetUnitOfWork(new List<RepositoryInterface>() { _repository }))
                {

                    _repository.Store(comment);
                    testFlier.NumberOfClaims++;
                }
            }
            FlierTestData.UpdateOne(testFlier, _repository, Kernel);

            var retClaims = _queryService.FindAggregateEntities<Claim>(retFlier.Id);
            AssertUtil.Count(5, retClaims);
            retClaims = retClaims.OrderBy(claim => claim.ClaimTime);
            //the first claims should be stored first
            CollectionAssert.AreEqual(claims, retClaims, new ClaimTestData.ClaimTestDataEq());

            retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            Assert.AreEqual(5, retFlier.NumberOfClaims);

        }

        [Test]
        public void AzureFlierRepositoryGetEntitiesClaimedByBrowserReturnsAllFlierClaimed()
        {
            var claims = new List<ClaimInterface>();
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            var claim = ClaimTestData.GetOne(Kernel, retFlier.Id);
            var browserId = claim.BrowserId;
            ClaimTestData.ClaimOne(testFlier, claim, _repository, Kernel);
            claims.Add(claim);

            testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            claim = ClaimTestData.GetOne(Kernel, retFlier.Id);
            claim.BrowserId = browserId;
            ClaimTestData.ClaimOne(testFlier, claim, _repository, Kernel);
            claims.Add(claim);

            var retClaims = _queryService.GetByBrowserId<Claim>(browserId);
            retClaims = retClaims.OrderByDescending(c => c.ClaimTime);
            AssertUtil.Count(2, retClaims);
            //the latest claims should be stored first
            CollectionAssert.AreEqual(claims.AsQueryable().Reverse(), retClaims, new ClaimTestData.ClaimTestDataEq());

        }

        private IQueryable<FlierInterface> AssertFindFliersByLocationTags(FlierSortOrder sortOrder)
        {
            var location = _loc;
            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            var retrievedFliers = _searchService.FindFliersByLocationAndDistance(location, distance: 10, take: 30, tags: tag, sortOrder: sortOrder)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).Where(f => f != null).AsQueryable();

            Assert.IsTrue(retrievedFliers.Any());

            AssertRetrievedFliersAreSameLocation(retrievedFliers);
            var list = retrievedFliers.ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                if (i > 0)
                    Assert.That(GetSorter(sortOrder)(list[i - 1]), Is.GreaterThanOrEqualTo(GetSorter(sortOrder)(list[i])));
            }

            return retrievedFliers;
        }

        private static Func<FlierInterface, object> GetSorter(FlierSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case FlierSortOrder.SortOrder:
                    return entry => entry.CreateDate.Ticks.ToString("D20");
            }
            return entry => entry.CreateDate;
        }

        [Test]
        public void FindFliersByLocationTagsByDifferentSortOrdersTest()
        {
            FindFliersByLocationTagsByDifferentSortOrders();
        }

        public FlierInterface FindFliersByLocationTagsByDifferentSortOrders()
        {
            DeleteAll();

            var flier = FlierTestData.GetOne(Kernel, _loc);
            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository });

            var indexer = Kernel.Get<SqlFlierIndexService>();
            using (uow)
            {
                var beh = FlierTestData.GetBehaviour(Kernel, flier);
                _repository.Store(flier);
                indexer.Handle(new FlierModifiedEvent() { NewState = flier });

                var earlierFlier = new Domain.Flier.Flier();
                earlierFlier.CopyFieldsFrom(flier);
                earlierFlier.CreateDate = earlierFlier.CreateDate.AddDays(-10);
                earlierFlier.Id = Guid.NewGuid().ToString();
                earlierFlier.NumberOfClaims = 1;
                _repository.Store(earlierFlier);
                indexer.Handle(new FlierModifiedEvent() { NewState = earlierFlier });

                var expiresLaterFlier = new Domain.Flier.Flier();
                expiresLaterFlier.CopyFieldsFrom(flier);
                expiresLaterFlier.CreateDate = earlierFlier.CreateDate.AddDays(-5);
                expiresLaterFlier.EffectiveDate = earlierFlier.EffectiveDate.AddDays(20);
                expiresLaterFlier.Id = Guid.NewGuid().ToString();
                expiresLaterFlier.NumberOfClaims = 2;
                _repository.Store(expiresLaterFlier);
                indexer.Handle(new FlierModifiedEvent() { NewState = expiresLaterFlier });


                //add fliers with variations on longitude and latitude
                var outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude);
                _repository.Store(outOfRangeFlier);
                indexer.Handle(new FlierModifiedEvent() { NewState = outOfRangeFlier });


                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);
                indexer.Handle(new FlierModifiedEvent() { NewState = outOfRangeFlier });


                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);
                indexer.Handle(new FlierModifiedEvent() { NewState = outOfRangeFlier });

            }

            Assert.IsTrue(uow.Successful);

            foreach (FlierSortOrder sortOrder in Enum.GetValues(typeof(FlierSortOrder)))
            {
                AssertFindFliersByLocationTags(sortOrder);
            }

            return flier;
        }


        public void FindFliersByLocationTagsPagedReturnsUniqueFliers()
        {
            DeleteAll();

            var flier = FlierTestData.GetOne(Kernel, _loc);
            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository });

            var indexer = Kernel.Get<SqlFlierIndexService>();
            using (uow)
            {
                _repository.Store(flier);
                indexer.Handle(new FlierModifiedEvent() { NewState = flier });

                var earlierFlier = new Domain.Flier.Flier();
                earlierFlier.CopyFieldsFrom(flier);
                earlierFlier.CreateDate = earlierFlier.CreateDate.AddDays(-10);
                earlierFlier.Id = Guid.NewGuid().ToString();
                earlierFlier.LocationRadius = 5;
                _repository.Store(earlierFlier);
                indexer.Handle(new FlierModifiedEvent() { NewState = earlierFlier });

                var earlierFlierSecond = new Domain.Flier.Flier();
                earlierFlierSecond.CopyFieldsFrom(flier);
                earlierFlierSecond.CreateDate = earlierFlier.CreateDate.AddDays(-5);
                earlierFlierSecond.Id = Guid.NewGuid().ToString();
                earlierFlierSecond.LocationRadius = 5;
                _repository.Store(earlierFlierSecond);
                indexer.Handle(new FlierModifiedEvent() { NewState = earlierFlierSecond });

                var earlierFlierThird = new Domain.Flier.Flier();
                earlierFlierThird.CopyFieldsFrom(flier);
                earlierFlierThird.CreateDate = earlierFlier.CreateDate.AddDays(-3);
                earlierFlierThird.Id = Guid.NewGuid().ToString();
                earlierFlierThird.LocationRadius = 5;
                _repository.Store(earlierFlierThird);
                indexer.Handle(new FlierModifiedEvent() { NewState = earlierFlierThird });

            }

            Assert.IsTrue(uow.Successful);

            foreach (FlierSortOrder sortOrder in Enum.GetValues(typeof(FlierSortOrder)))
            {
                AssertFindFliersByLocationTags(sortOrder);
            }
        }

        private void AssertRetrievedFliersAreSameLocation(IQueryable<FlierInterface> retrievedFliers)
        {
            foreach (var retrievedFlierCombo in from r1 in retrievedFliers
                                                from r2 in retrievedFliers
                                                select new { r1, r2})
            {
                Assert.AreEqual(retrievedFlierCombo.r1.Location, retrievedFlierCombo.r2.Location);
            }
        }
    }


}
