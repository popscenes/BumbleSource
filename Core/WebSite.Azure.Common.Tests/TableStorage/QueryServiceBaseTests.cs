﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Ninject;
using Ninject.MockingKernel.Moq;
using WebSite.Azure.Common.TableStorage;

namespace WebSite.Azure.Common.Tests.TableStorage
{
    [TestFixture]
    public class QueryServiceBaseTests
    {
        static MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        Dictionary<string, List<JsonTableEntry>> _mockStore;

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<TableNameAndPartitionProviderServiceInterface>()
                .To<TableNameAndPartitionProviderService>()
                .InSingletonScope();

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
            tableNameAndPartitionProviderService.Add<OneEntity>(0, "testOneEntity", entity => entity.Id);
            tableNameAndPartitionProviderService.Add<OneEntity>(1, "testOneEntity", entity => entity.PropTwo, entity => entity.Prop);
            tableNameAndPartitionProviderService.Add<OneEntity>(2, "testOneEntity", entity => entity.Prop + entity.PropTwo, entity => entity.PropTwo);

            tableNameAndPartitionProviderService.Add<TwoEntity>(0, "testTwoEntity", entity => entity.Id);
            tableNameAndPartitionProviderService.Add<TwoEntity>(10, "testTwoEntity", entity => entity.PropTwo, entity => entity.Prop);//using PropTwo as aggregate partition id

            tableNameAndPartitionProviderService.Add<ThreeEntity>(0, "testThreeEntity", entity => entity.SomeProp.ToString(CultureInfo.InvariantCulture));

            _mockStore = TableContextTests.SetupMockTableContext<JsonTableEntry>(Kernel, new Dictionary<string, List<JsonTableEntry>>());


            Kernel.Bind<TestRespositoryBase<JsonTableEntry>>()
                .ToSelf().InTransientScope();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableContextInterface>();
            Kernel.Unbind<TableNameAndPartitionProviderServiceInterface>();
            Kernel.Unbind<TestRespositoryBase<JsonTableEntry>>();
        }

        [Test]
        public void QueryServiceBaseReturnsEntityForFindById()
        {
            var aggregateId = Guid.NewGuid().ToString();
            var one = new OneEntity()
            {
                Id = aggregateId,
                Prop = "Ya",
                PropTwo = "You",
                PropThree = "My property",
                MemberEntity = new ThreeEntity() { SomeProp = 45, MemberEntity = new TwoEntity() { Prop = "ThreeMember", PropTwo = "ThreeMemberTwo" } },
                RelatedEntities = new List<TwoEntity>()
                                      {
                                          new TwoEntity() {Id = Guid.NewGuid().ToString(), Prop = "123", PropTwo = aggregateId }, 
                                          new TwoEntity() {Id = Guid.NewGuid().ToString(), Prop = "555", PropTwo = aggregateId }
                                      }
            };

            var repo = Kernel.Get<TestRespositoryBase<JsonTableEntry>>();
            repo.Store(one);
            repo.SaveChanges();

            var qs = Kernel.Get<QueryServiceBase<JsonTableEntry>>();
            var ret = qs.FindById<OneEntity>(one.Id);

            Assert.IsNotNull(ret);

            OneEntity.AssertAreEqual(one, ret);

        }

        [Test]
        public void QueryServiceBaseFindsRelatedEntities()
        {
            var aggregateId = Guid.NewGuid().ToString();
            var one = new OneEntity()
            {
                Id = aggregateId,
                Prop = "Ya",
                PropTwo = "You",
                PropThree = "My property",
                MemberEntity = new ThreeEntity() { SomeProp = 45, MemberEntity = new TwoEntity() { Prop = "ThreeMember", PropTwo = "ThreeMemberTwo" } },
                RelatedEntities = new List<TwoEntity>()
                                      {
                                          new TwoEntity() {Id = Guid.NewGuid().ToString(), Prop = "123", PropTwo = aggregateId }, 
                                          new TwoEntity() {Id = Guid.NewGuid().ToString(), Prop = "555", PropTwo = aggregateId }
                                      }
            };

            var repo = Kernel.Get<TestRespositoryBase<JsonTableEntry>>();
            repo.Store(one);
            repo.SaveChanges();

            var qs = Kernel.Get<QueryServiceBase<JsonTableEntry>>();
            var ret = qs.FindAggregateEntities<TwoEntity>(aggregateId);

            Assert.Count(2, ret);
            Assert.AreElementsEqualIgnoringOrder(one.RelatedEntities, ret);
        }
    }
}
