using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Ninject;
using WebSite.Azure.Common.Environment;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Flier;
using PostaFlya.DataRepository.Search.Implementation;
using PostaFlya.DataRepository.Tests.Behaviour.TaskJob;
using PostaFlya.DataRepository.Tests.Internal;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Likes;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using PostaFlya.Domain.TaskJob;
using WebSite.Infrastructure.Command;
using PostaFlya.Mocks.Domain.Data;

namespace PostaFlya.DataRepository.Tests
{
    [TestFixture]
    public class AzureFlierRepositoryTests
    {
        private FlierRepositoryInterface _repository;
        private FlierQueryServiceInterface _queryService;

        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        private string _env;
        [Row("dev")] 
        [Row("real")]
        public AzureFlierRepositoryTests(string env)
        {
            _env = env;
            AzureEnv.UseRealStorage = env == "real";
        } 


        [FixtureSetUp]
        public void FixtureSetUp()
        {
            new AzureCommentRepositoryTests(_env).FixtureSetUp();
            new AzureLikeRepositoryTests(_env).FixtureSetUp();

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
            
            _repository = Kernel.Get<FlierRepositoryInterface>();
            _queryService = Kernel.Get<FlierQueryServiceInterface>();          
        }

        [FixtureTearDown]
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
            var repository = Kernel.Get<FlierRepositoryInterface>();
            Assert.IsNotNull(repository);
            Assert.IsInstanceOfType<AzureFlierRepository>(repository);

            var queryService = Kernel.Get<FlierQueryServiceInterface>();
            Assert.IsNotNull(queryService);
            Assert.IsInstanceOfType<AzureFlierRepository>(queryService);
        }

        private static Location _loc = new Location(55,55);
        [Test]
        public FlierInterface TestStoreFlierRepository()
        {
            DeleteAll();

            var flier = FlierTestData.GetOne(Kernel, _loc);
            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {_repository});
            using (uow)
            {
                var beh = FlierTestData.GetBehaviour(Kernel, flier);
                _repository.Store(flier);

                var earlierFlier = new Domain.Flier.Flier();
                earlierFlier.CopyFieldsFrom(flier);
                earlierFlier.CreateDate = earlierFlier.CreateDate.AddDays(-1);
                earlierFlier.Id = Guid.NewGuid().ToString();
                _repository.Store(earlierFlier);

                //add fliers with variations on longitude and latitude
                var outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude);
                _repository.Store(outOfRangeFlier);

                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);

                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);
            }

            Assert.IsTrue(uow.Successful);

            return flier;
        }

        [Test]
        public FlierInterface TestGetByIdFlierRepository()
        {
            var storedFlier = TestStoreFlierRepository();
            return FlierTestData.AssertGetById(storedFlier, _queryService);
        }

        [Test]
        public IQueryable<FlierInterface> TestFindFliersByLocationAndTagsRepository()
        {
            var storedFlier = TestStoreFlierRepository();

            var location = _loc;
            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            var retrievedFliers = _queryService.FindFliersByLocationTagsAndDistance(location, tag)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).AsQueryable();

            Assert.IsTrue(retrievedFliers.Any());
            AssertRetrievedFliersAreSameLocation(retrievedFliers);


            retrievedFliers = _queryService.FindFliersByLocationTagsAndDistance(new Location(130, 130), tag)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).AsQueryable();
            Assert.IsTrue(!retrievedFliers.Any());

            var theBadTags = new Tags(){"crapolla","shitolla"};
            retrievedFliers = _queryService.FindFliersByLocationTagsAndDistance(location, theBadTags)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).AsQueryable();
            Assert.IsTrue(!retrievedFliers.Any());

            return retrievedFliers;
        }

        [Test]
        public IQueryable<FlierInterface> FlierRepositoryGetByBrowserId()
        {
            var storedFlier = TestStoreFlierRepository();
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

            Assert.AreElementsEqualIgnoringOrder(retFlier.ExtendedProperties["taskjob"].Properties
                , testFlier.ExtendedProperties["taskjob"].Properties);
        }


        [Test]
        public void AzureFlierRepositoryRetriedUpdateIfConcurrencyExceptionOccurs()
        {
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);

            var tryCount     = 0;
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = Kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new List<RepositoryInterface>() {_repository}))
            {

                _repository.UpdateEntity<Domain.Flier.Flier>(testFlier.Id
                    , flier =>
                        {
                            if(tryCount++ == 0)
                            {
                                var otherrepo = Kernel.Get<FlierRepositoryInterface>();
                                otherrepo.UpdateEntity<Domain.Flier.Flier>(testFlier.Id, f => f.NumberOfComments++);
                                otherrepo.SaveChanges();
                            }

                            flier.NumberOfComments++;

                        }

                    );
            }

            Assert.IsTrue(unitOfWork.Successful);

            testFlier.NumberOfComments = 2;//this is what they should be
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            Assert.AreEqual(2, retFlier.NumberOfComments);

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
            Assert.Count(5, retComments);
            Assert.AreElementsEqual(comments, retComments, CommentTestData.Equals);

            retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            Assert.AreEqual(5, retFlier.NumberOfComments);

        }

        [Test]
        public void AzureFlierRepositoryLikeUpdatesNumberOfLikes()
        {
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);

            var like = LikeTestData.GetOne(Kernel, retFlier.Id);

            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository }))
            {

                _repository.Store(like);
                testFlier.NumberOfLikes++;
            }
            FlierTestData.UpdateOne(testFlier, _repository, Kernel);

            retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            Assert.AreEqual(1, retFlier.NumberOfLikes);

        }


        [Test]
        public void AzureFlierRepositoryGetLikesReturnsAllLikesOnAFlier()
        {
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);


            var likes = LikeTestData.GetSome(Kernel, retFlier.Id, 5);
            foreach (var comment in likes)
            {
                using (Kernel.Get<UnitOfWorkFactoryInterface>()
                    .GetUnitOfWork(new List<RepositoryInterface>() { _repository }))
                {

                    _repository.Store(comment);
                    testFlier.NumberOfLikes++;
                }
            }
            FlierTestData.UpdateOne(testFlier, _repository, Kernel);

            var retLikes = _queryService.FindAggregateEntities<Like>(retFlier.Id);
            Assert.Count(5, retLikes);
            //the first likes should be stored first
            Assert.AreElementsEqual(likes, retLikes, LikeTestData.Equals);

            retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            Assert.AreEqual(5, retFlier.NumberOfLikes);

        }

        [Test]
        public void AzureFlierRepositoryGetEntitiesLikedByBrowserReturnsAllFlierLiked()
        {
            var likes = new List<LikeInterface>();
            var testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            var retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            var like = LikeTestData.GetOne(Kernel, retFlier.Id);
            var browserId = like.BrowserId;
            LikeTestData.LikeOne(testFlier, like, _repository, Kernel);
            likes.Add(like);

            testFlier = FlierTestData.GetOne(Kernel);
            testFlier.FlierBehaviour = FlierBehaviour.Default;
            testFlier = FlierTestData.StoreOne(testFlier, _repository, Kernel);
            retFlier = FlierTestData.AssertGetById(testFlier, _queryService);
            like = LikeTestData.GetOne(Kernel, retFlier.Id);
            like.BrowserId = browserId;
            LikeTestData.LikeOne(testFlier, like, _repository, Kernel);
            likes.Add(like);

            var retLikes = _queryService.GetByBrowserId<Like>(browserId);
            Assert.Count(2, retLikes);
            //the latest likes should be stored first
            Assert.AreElementsEqual(likes.AsQueryable().Reverse(), retLikes, LikeTestData.Equals);
        }

        private IQueryable<FlierInterface> AssertFindFliersByLocationTags(FlierSortOrder sortOrder)
        {
            var location = _loc;
            var tag = Kernel.Get<Tags>(bm => bm.Has("default"));

            var retrievedFliers = _queryService.FindFliersByLocationTagsAndDistance(location, tag, 10, 0, sortOrder)
                .Select(id => _queryService.FindById<Domain.Flier.Flier>(id)).Where(f => f != null).AsQueryable();

            Assert.IsTrue(retrievedFliers.Any());

            AssertRetrievedFliersAreSameLocation(retrievedFliers);
            var list = retrievedFliers.ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                if (i > 0)
                    Assert.GreaterThanOrEqualTo(TableFlierSearchService.GetSorter(sortOrder)(list[i - 1]), TableFlierSearchService.GetSorter(sortOrder)(list[i]));
            }

            return retrievedFliers;
        }

        [Test]
        public FlierInterface FindFliersByLocationTagsByDifferentSortOrders()
        {
            DeleteAll();

            var flier = FlierTestData.GetOne(Kernel, _loc);
            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _repository });
            using (uow)
            {
                var beh = FlierTestData.GetBehaviour(Kernel, flier);
                _repository.Store(flier);

                var earlierFlier = new Domain.Flier.Flier();
                earlierFlier.CopyFieldsFrom(flier);
                earlierFlier.CreateDate = earlierFlier.CreateDate.AddDays(-10);
                earlierFlier.Id = Guid.NewGuid().ToString();
                earlierFlier.NumberOfLikes = 1;
                _repository.Store(earlierFlier);

                var expiresLaterFlier = new Domain.Flier.Flier();
                expiresLaterFlier.CopyFieldsFrom(flier);
                expiresLaterFlier.CreateDate = earlierFlier.CreateDate.AddDays(-5);
                expiresLaterFlier.EffectiveDate = earlierFlier.EffectiveDate.AddDays(20);
                expiresLaterFlier.Id = Guid.NewGuid().ToString();
                expiresLaterFlier.NumberOfLikes = 2;
                _repository.Store(expiresLaterFlier);

                //add fliers with variations on longitude and latitude
                var outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude);
                _repository.Store(outOfRangeFlier);

                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);

                outOfRangeFlier = new Domain.Flier.Flier();
                outOfRangeFlier.CopyFieldsFrom(flier);
                outOfRangeFlier.Id = Guid.NewGuid().ToString();
                outOfRangeFlier.Location = new Location(flier.Location.Longitude + 10, flier.Location.Latitude + 10);
                _repository.Store(outOfRangeFlier);
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
