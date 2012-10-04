using System;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Azure.Common.Environment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Test.Common;
using Website.Domain.Comments;
using Website.Mocks.Domain.Data;

namespace PostaFlya.DataRepository.Tests.Internal
{
    [TestFixture("dev")]
    [TestFixture("real")]
    public class AzureCommentRepositoryTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public AzureCommentRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 

        GenericRepositoryInterface _repository;
        GenericQueryServiceInterface _queryService;


        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(new TableNameAndPartitionProvider<CommentInterface>()
//                            {
//                            {typeof(CommentTableEntry), CommentStorageDomain.IdPartition, "commentsTest", i => i.Id, CommentStorageDomain.GetIdKey},
//                            {typeof(CommentTableEntry), CommentStorageDomain.AggregateIdPartition, "commentsTest", i => i.EntityId, CommentStorageDomain.GetIdKey},
//                            })
//                .WhenAnyAnchestorNamed("comments")
//                .InSingletonScope();
//
//            
//            var context = Kernel.Get<AzureTableContext>("comments");
//            context.InitFirstTimeUse();
//            context.Delete<CommentTableEntry>(null, CommentStorageDomain.IdPartition);
//            context.Delete<CommentTableEntry>(null, CommentStorageDomain.AggregateIdPartition);
//            context.SaveChanges();

            _repository = Kernel.Get<GenericRepositoryInterface>();
            _queryService = Kernel.Get<GenericQueryServiceInterface>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            //Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        [Test]
        public void AzureCommentRepositoryCreate()
        {
            var repository = Kernel.Get<GenericRepositoryInterface>();
            Assert.IsNotNull(repository);
        }

        public CommentInterface AzureCommentRepositoryStore()
        {
            var comment = CommentTestData.GetOne(Kernel, Guid.NewGuid().ToString());
            CommentTestData.StoreOne(comment, _repository, Kernel);
            return comment;
        }
        [Test]
        public void AzureCommentRepositoryStoreTest()
        {
            AzureCommentRepositoryStore();
        }

        [Test]
        public void AzureCommentRepositoryQueryTest()
        {
            AzureCommentRepositoryQuery();
        }

        [Test]
        public void AzureCommentRepositoryGetByEntity()
        {
            var entityId = Guid.NewGuid().ToString();
            var stored = CommentTestData.StoreSome(_repository, Kernel, entityId);
            var retd = _queryService.FindAggregateEntities<Comment>(entityId);
            CollectionAssert.AreEqual(stored, retd, new CommentTestData.CommentTestDataEq());
        }

        [Test]
        public void AzureCommentRepositoryGetTopByEntity()
        {
            var entityId = Guid.NewGuid().ToString();
            var stored = CommentTestData.StoreSome(_repository, Kernel, entityId);
            var retd = _queryService.FindAggregateEntities<Comment>(entityId, 3);
            AssertUtil.Count(3, retd);
            CollectionAssert.AreEqual(stored.Take(3), retd, new CommentTestData.CommentTestDataEq());
            AssertUtil.AssertAdjacentElementsAre(retd, (current, next) => current.CommentTime < next.CommentTime );
        }

        public CommentInterface AzureCommentRepositoryQuery()
        {
            var comment = AzureCommentRepositoryStore();
            var commentRet = CommentTestData.AssertGetById(comment, _queryService);
            return commentRet;
        }

    }
}
