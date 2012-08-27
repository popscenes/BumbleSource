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
using Website.Domain.Claims;
using Website.Mocks.Domain.Data;

namespace PostaFlya.DataRepository.Tests.Internal
{
    [TestFixture]
    public class AzureClaimRepositoryTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Row("dev")] 
        [Row("real")]
        public AzureClaimRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 

        GenericRepositoryInterface _repository;
        GenericQueryServiceInterface _queryservice;


        [FixtureSetUp]
        public void FixtureSetUp()
        {
            _repository = Kernel.Get<GenericRepositoryInterface>();
            _queryservice = Kernel.Get<GenericQueryServiceInterface>();
        }

        [FixtureTearDown]
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
        public ClaimInterface AzureClaimRepositoryStore()
        {
            var claim = ClaimTestData.GetOne(Kernel, Guid.NewGuid().ToString());
            ClaimTestData.StoreOne(claim, _repository, Kernel);
            return claim;
        }

        [Test]
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
            Assert.AreElementsEqualIgnoringOrder(stored, retd, ClaimTestData.Equals);
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
