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
using PostaFlya.DataRepository.Internal;
using PostaFlya.Mocks.Domain.Data;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;
using WebSite.Test.Common;
using Website.Domain.Likes;
using Website.Mocks.Domain.Data;

namespace PostaFlya.DataRepository.Tests.Internal
{
    [TestFixture]
    public class AzureLikeRepositoryTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Row("dev")] 
        [Row("real")]
        public AzureLikeRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 

        GenericRepositoryInterface _repository;
        GenericQueryServiceInterface _queryservice;


        [FixtureSetUp]
        public void FixtureSetUp()
        {
//            Kernel.Bind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(new TableNameAndPartitionProvider<LikeInterface>()
//                            {
//                                             {typeof(LikeTableEntry), LikeStorageDomain.IdPartition, "likeTest", LikeStorageDomain.GetIdPartitionKey, i => i.BrowserId},
//                                             {typeof(LikeTableEntry), LikeStorageDomain.AggregateIdPartition, "likeTest", i => i.EntityId, LikeStorageDomain.GetAggregateRowKey},
//                                             {typeof(LikeTableEntry), LikeStorageDomain.BrowserIdPartition, "likeTest", i => i.BrowserId, LikeStorageDomain.GetBrowserRowKey},
//                            })
//                .WhenAnyAnchestorNamed("likes")
//                .InSingletonScope();
//
//
//            var context = Kernel.Get<AzureTableContext>("likes");
//            context.InitFirstTimeUse();
//            context.Delete<LikeTableEntry>(null, LikeStorageDomain.IdPartition);
//            context.Delete<LikeTableEntry>(null, LikeStorageDomain.AggregateIdPartition);
//            context.Delete<LikeTableEntry>(null, LikeStorageDomain.BrowserIdPartition);
//            context.SaveChanges();

            _repository = Kernel.Get<GenericRepositoryInterface>();
            _queryservice = Kernel.Get<GenericQueryServiceInterface>();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            //Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        [Test]
        public void AzureLikeRepositoryCreate()
        {
            var repository = Kernel.Get<GenericRepositoryInterface>();
            Assert.IsNotNull(repository);
        }

        [Test]
        public LikeInterface AzureLikeRepositoryStore()
        {
            var like = LikeTestData.GetOne(Kernel, Guid.NewGuid().ToString());
            LikeTestData.StoreOne(like, _repository, Kernel);
            return like;
        }

        [Test]
        public LikeInterface AzureLikeRepositoryQuery()
        {
            var like = AzureLikeRepositoryStore();
            var likeRet = LikeTestData.AssertGetById(like, _queryservice);
            return likeRet;
        }

        [Test]
        public void AzureLikeRepositoryGetByEntity()
        {
            var entityId = Guid.NewGuid().ToString();
            var stored = LikeTestData.StoreSome(_repository, Kernel, entityId, 5);
            var retd = _queryservice.FindAggregateEntities<Like>(entityId);
            Assert.AreElementsEqualIgnoringOrder(stored, retd, LikeTestData.Equals);
        }

        [Test]
        public void NullLikeContentIsOkForLike()
        {
            var like = LikeTestData.GetOne(Kernel, Guid.NewGuid().ToString());
            like.LikeContent = null;
            LikeTestData.StoreOne(like, _repository, Kernel);
        }
    }
}
