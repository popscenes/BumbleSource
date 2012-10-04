using System;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Azure.Common.Environment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Test.Common;
using Website.Domain.Claims;
using Website.Mocks.Domain.Data;

namespace PostaFlya.DataRepository.Tests.Internal
{
    [TestFixture("dev")]
    [TestFixture("real")]
    public class AzureClaimRepositoryTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public AzureClaimRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 

        GenericRepositoryInterface _repository;
        GenericQueryServiceInterface _queryservice;


        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _repository = Kernel.Get<GenericRepositoryInterface>();
            _queryservice = Kernel.Get<GenericQueryServiceInterface>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            AzureEnv.UseRealStorage = false;
        }

        [Test]
        public void AzureClaimRepositoryCreate()
        {
            var repository = Kernel.Get<GenericRepositoryInterface>();
            Assert.IsNotNull(repository);
        }

        [Test]
        public void AzureClaimRepositoryStoreTest()
        {
            AzureClaimRepositoryStore();
        }

        public ClaimInterface AzureClaimRepositoryStore()
        {
            var claim = ClaimTestData.GetOne(Kernel, Guid.NewGuid().ToString());
            ClaimTestData.StoreOne(claim, _repository, Kernel);
            return claim;
        }

        [Test]
        public void AzureClaimRepositoryQueryTest()
        {
            AzureClaimRepositoryQuery();
        }

        public ClaimInterface AzureClaimRepositoryQuery()
        {
            var claim = AzureClaimRepositoryStore();
            var claimRet = ClaimTestData.AssertGetById(claim, _queryservice);
            return claimRet;
        }

        [Test]
        public void AzureClaimRepositoryGetByEntity()
        {
            var entityId = Guid.NewGuid().ToString();
            var stored = ClaimTestData.StoreSome(_repository, Kernel, entityId, 5);
            var retd = _queryservice.FindAggregateEntities<Claim>(entityId);
            AssertUtil.AreEquivalent(stored, retd, ClaimTestData.Equals);
        }

        [Test]
        public void NullClaimContentIsOkForClaim()
        {
            var claim = ClaimTestData.GetOne(Kernel, Guid.NewGuid().ToString());
            claim.ClaimContext = null;
            ClaimTestData.StoreOne(claim, _repository, Kernel);
        }
    }
}
