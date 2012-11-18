using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using PostaFlya.DataRepository.Internal;
using PostaFlya.DataRepository.Search.Command;
using PostaFlya.DataRepository.Search.Event;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Flier.Event;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.Environment;
using PostaFlya.DataRepository.Search.Implementation;
using PostaFlya.DataRepository.Tests.Behaviour.TaskJob;
using PostaFlya.DataRepository.Tests.Internal;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.TaskJob;
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
    public class AzureFlierRepositoryTests
    {
        private GenericRepositoryInterface _repository;
        private QueryServiceForBrowserAggregateInterface _queryService;
        private FlierSearchServiceInterface _searchService;

        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        private string _env;
        public AzureFlierRepositoryTests(string env)
        {
            _env = env;
            AzureEnv.UseRealStorage = env == "real";
        } 


        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            new AzureCommentRepositoryTests(_env).FixtureSetUp();
            new AzureClaimRepositoryTests(_env).FixtureSetUp();

//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(new TableNameAndPartitionProvider<FlierInterface>()
//                                                                {
//                                        {typeof(FlierTableEntry), FlierStorageDomain.IdPartition, "flierbyidTest", f => f.Id, f=>f.Id}, 
//                                        {typeof(FlierTableEntry), FlierStorageDomain.BrowserPartition, "flierbyidTest", f => f.BrowserId, f=>f.Id}, 
////                                        {typeof(FlierTableEntry), LocationPartition, "flierbyloc", f => GetLocationPartitionKey(f.Location), f=>f.Id},
//
////                                        {typeof(FlierSearchEntry), FlierStorageDomain.CreatedDateSearchPartition, "flierbycreatedTest", f => FlierStorageDomain.GetCoarseLocationPartitionKey(f.Location), f=> FlierStorageDomain.GetIdByCreated(f)},
////                                        {typeof(FlierSearchEntry), FlierStorageDomain.EffectiveDateSearchPartition, "flierbyeffectiveTest", f => FlierStorageDomain.GetCoarseLocationPartitionKey(f.Location), f=> FlierStorageDomain.GetIdByEffectiveDate(f)},
////                                        {typeof(FlierSearchEntry), FlierStorageDomain.PopularitySearchPartition, "flierbypopularTest", f => FlierStorageDomain.GetCoarseLocationPartitionKey(f.Location), f=> FlierStorageDomain.GetIdByPopular(f)}
//                                    })
//                .WhenAnyAnchestorNamed("flier");
//
//
//            var context = Kernel.Get<AzureTableContext>("flier");
//            context.InitFirstTimeUse();
            Kernel.Get<SqlSeachDbInitializer>().Initialize();
            DeleteAll();

            TaskJobRepositoryTests.BindTaskJobRepository(Kernel);
            
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

            var flier = FlierTestData.GetOne(Kernel, _loc);
            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {_repository});

            var indexer = Kernel.Get<SqlFlierIndexService>();
            using (uow)
            {
                var beh = FlierTestData.GetBehaviour(Kernel, flier);
                _repository.Store(flier);
                indexer.Publish(new FlierModifiedEvent(){NewState = flier});

                var earlierFlier = new Domain.Flier.Flier();
                earlierFlier.CopyFieldsFrom(flier);
                earlierFlier.CreateDate = earlierFlier.CreateDate.AddDays(-1);
                earlierFlier.Id = Guid.NewGuid().ToString();
                _repository.Store(earlierFlier);
                indexer.Publish(new FlierModifiedEvent() { NewState = earlierFlier });

                //add fliers with variations on longitude and latitude
                var outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude);
                _repository.Store(outOfRangeFlier);
                indexer.Publish(new FlierModifiedEvent() { NewState = outOfRangeFlier });

                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);
                indexer.Publish(new FlierModifiedEvent() { NewState = outOfRangeFlier });


                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);
                indexer.Publish(new FlierModifiedEvent() { NewState = outOfRangeFlier });

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
        public void FindFliersByLocationAndTagsAndBoard()
        {
            var storedFlier = StoreFlierRepository();
            var board = BoardTestData.GetOne(Kernel, "TestBoardName", _loc);
            board = BoardTestData.StoreOne(board, _repository, Kernel);

            var boardFlier = new BoardFlier()
                {
                    Id = storedFlier.Id + board.Id,
                    AggregateId = board.Id,
                    FlierId = storedFlier.Id,
                    Status = BoardFlierStatus.Approved
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

            indexer.Publish(new FlierModifiedEvent() { NewState = _queryService.FindById<Domain.Flier.Flier>(storedFlier.Id), OrigState = storedFlier});
            indexer.Publish(new BoardFlierModifiedEvent() { NewState = boardFlier });

            var location = _loc;
            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            var retrievedFliers = _searchService.FindFliersByLocationTagsAndDistance(location, tag, board.Id)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(retrievedFliers.Count(), Is.EqualTo(1));

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFliers.FirstOrDefault());
        }

        [Test]
        public void FindFliersByTagsAndBoard()
        {
            var storedFlier = StoreFlierRepository();
            var board = BoardTestData.GetOne(Kernel, "TestBoardNameNoLoc", _loc);
            board = BoardTestData.StoreOne(board, _repository, Kernel);

            var boardFlier = new BoardFlier()
            {
                Id = storedFlier.Id + board.Id,
                AggregateId = board.Id,
                FlierId = storedFlier.Id,
                Status = BoardFlierStatus.Approved
            };

            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository });

            var indexer = Kernel.Get<SqlFlierIndexService>();
            using (uow)
            {
                _repository.Store(boardFlier);
                indexer.Publish(new BoardFlierModifiedEvent() { NewState = boardFlier });
            }

            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            var retrievedFliers = _searchService.FindFliersByLocationTagsAndDistance(new Location(), tag, board.Id)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).ToList();

            Assert.That(retrievedFliers.Count(), Is.EqualTo(1));

            FlierTestData.AssertStoreRetrieve(storedFlier, retrievedFliers.FirstOrDefault());
        }

        public IQueryable<FlierInterface> FindFliersByLocationAndTagsRepository()
        {
            var storedFlier = StoreFlierRepository();

            var location = _loc;
            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            var retrievedFliers = _searchService.FindFliersByLocationTagsAndDistance(location, tag)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).AsQueryable();

            Assert.IsTrue(retrievedFliers.Any());
            AssertRetrievedFliersAreSameLocation(retrievedFliers);


            retrievedFliers = _searchService.FindFliersByLocationTagsAndDistance(new Location(130, 130), tag)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).AsQueryable();
            Assert.IsTrue(!retrievedFliers.Any());

            var theBadTags = new Tags(){"crapolla","shitolla"};
            retrievedFliers = _searchService.FindFliersByLocationTagsAndDistance(location, theBadTags)
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
        public void FlierQueryAndStoreCorrectlyCreatesAndSerialisesTaskJobBehaviourProperties()
        {
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.TaskJob;
            var behaviour = FlierTestData.GetBehaviour(Kernel, testFlier) as TaskJobFlierBehaviourInterface;

            behaviour.CostOverhead = 111;

            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);

            CollectionAssert.AreEquivalent(retFlier.ExtendedProperties
                , testFlier.ExtendedProperties);

            AssertUtil.Count(1, retFlier.ExtendedProperties);
            var behaviourRet = FlierTestData.GetBehaviour(Kernel, testFlier) as TaskJobFlierBehaviourInterface;
            Assert.AreEqual(111, behaviourRet.CostOverhead);
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

            var retrievedFliers = _searchService.FindFliersByLocationTagsAndDistance(location, tag, null, 10, 0, sortOrder)
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
                case FlierSortOrder.CreatedDate:
                    return entry => entry.CreateDate.Ticks.ToString("D20") + '[' + (entry.NumberOfClaims + entry.NumberOfComments).ToString("D10") + ']';
                case FlierSortOrder.EffectiveDate:
                    return entry => entry.EffectiveDate.Ticks.ToString("D20") + '[' + (entry.NumberOfClaims + entry.NumberOfComments).ToString("D10") + ']';
                case FlierSortOrder.Popularity:
                    return entry => (entry.NumberOfClaims + entry.NumberOfComments).ToString("D10") + '[' + entry.CreateDate.Ticks.ToString("D20") + ']';
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
                indexer.Publish(new FlierModifiedEvent() { NewState = flier });

                var earlierFlier = new Domain.Flier.Flier();
                earlierFlier.CopyFieldsFrom(flier);
                earlierFlier.CreateDate = earlierFlier.CreateDate.AddDays(-10);
                earlierFlier.Id = Guid.NewGuid().ToString();
                earlierFlier.NumberOfClaims = 1;
                _repository.Store(earlierFlier);
                indexer.Publish(new FlierModifiedEvent() { NewState = earlierFlier });

                var expiresLaterFlier = new Domain.Flier.Flier();
                expiresLaterFlier.CopyFieldsFrom(flier);
                expiresLaterFlier.CreateDate = earlierFlier.CreateDate.AddDays(-5);
                expiresLaterFlier.EffectiveDate = earlierFlier.EffectiveDate.AddDays(20);
                expiresLaterFlier.Id = Guid.NewGuid().ToString();
                expiresLaterFlier.NumberOfClaims = 2;
                _repository.Store(expiresLaterFlier);
                indexer.Publish(new FlierModifiedEvent() { NewState = expiresLaterFlier });


                //add fliers with variations on longitude and latitude
                var outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude);
                _repository.Store(outOfRangeFlier);
                indexer.Publish(new FlierModifiedEvent() { NewState = outOfRangeFlier });


                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);
                indexer.Publish(new FlierModifiedEvent() { NewState = outOfRangeFlier });


                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);
                indexer.Publish(new FlierModifiedEvent() { NewState = outOfRangeFlier });

            }

            Assert.IsTrue(uow.Successful);

            foreach (FlierSortOrder sortOrder in Enum.GetValues(typeof(FlierSortOrder)))
            {
                AssertFindFliersByLocationTags(sortOrder);
            }

            return flier;
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
