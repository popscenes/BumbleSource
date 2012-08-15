using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using Ninject.MockingKernel.Moq;
using WebSite.Azure.Common.Environment;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Flier;
using PostaFlya.DataRepository.Internal;
using PostaFlya.Domain.Comments;
using PostaFlya.Mocks.Domain.Data;
using WebSite.Test.Common;

namespace PostaFlya.DataRepository.Tests.Internal
{
    [TestFixture]
    public class AzureCommentRepositoryTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Row("dev")] 
        [Row("real")]
        public AzureCommentRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 

        AzureCommentRepository _repository;


        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<TableNameAndPartitionProviderInterface>()
                .ToConstant(new TableNameAndPartitionProvider<CommentInterface>()
                            {
                            {typeof(CommentTableEntry), CommentStorageDomain.IdPartition, "commentsTest", i => i.Id, CommentStorageDomain.GetIdKey},
                            {typeof(CommentTableEntry), CommentStorageDomain.AggregateIdPartition, "commentsTest", i => i.EntityId, CommentStorageDomain.GetIdKey},
                            })
                .WhenAnyAnchestorNamed("comments")
                .InSingletonScope();

            
            var context = Kernel.Get<AzureTableContext>("comments");
            context.InitFirstTimeUse();
            context.Delete<CommentTableEntry>(null, CommentStorageDomain.IdPartition);
            context.Delete<CommentTableEntry>(null, CommentStorageDomain.AggregateIdPartition);
            context.SaveChanges();

            _repository = Kernel.Get<AzureCommentRepository>();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        [Test]
        public void AzureCommentRepositoryCreate()
        {
            var repository = Kernel.Get<AzureCommentRepository>();
            Assert.IsNotNull(repository);
        }

        [Test]
        public CommentInterface AzureCommentRepositoryStore()
        {
            var comment = CommentTestData.GetOne(Kernel, Guid.NewGuid().ToString());
            CommentTestData.StoreOne(comment, _repository, Kernel);
            return comment;
        }

        [Test]
        public CommentInterface AzureCommentRepositoryQuery()
        {
            var comment = AzureCommentRepositoryStore();
            var commentRet = CommentTestData.AssertGetById(comment, _repository);
            return commentRet;
        }

        [Test]
        public void AzureCommentRepositoryGetByEntity()
        {
            var entityId = Guid.NewGuid().ToString();
            var stored = CommentTestData.StoreSome(_repository, Kernel, entityId);
            var retd = _repository.GetByEntity(entityId);
            Assert.AreElementsEqualIgnoringOrder(stored, retd, CommentTestData.Equals);
        }

        [Test]
        public void AzureCommentRepositoryGetTopByEntity()
        {
            var entityId = Guid.NewGuid().ToString();
            var stored = CommentTestData.StoreSome(_repository, Kernel, entityId);
            var retd = _repository.GetByEntity(entityId, 3);
            Assert.Count(3, retd);
            Assert.AreElementsEqualIgnoringOrder(stored.Take(3), retd, CommentTestData.Equals);
            AssertUtil.AssertAdjacentElementsAre(retd, (current, next) => current.CommentTime < next.CommentTime );
        }

    }
}
