﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Azure.Common.TableStorage;
using Website.Test.Common;

namespace Website.Azure.Common.Tests.TableStorage
{
    [TestFixture]
    public class QueryServiceBaseTests
    {
        static MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        Dictionary<string, List<JsonTableEntry>> _mockStore;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<TableNameAndIndexProviderServiceInterface>()
                .To<TableNameAndIndexProviderService>()
                .InSingletonScope();

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();
            tableNameAndPartitionProviderService.Add<OneEntity>("testOneEntity", entity => entity.Id);

            tableNameAndPartitionProviderService.Add<TwoEntity>("testTwoEntity", entity => entity.AggregateId, entity => entity.Id);

            tableNameAndPartitionProviderService.Add<ThreeEntity>("testThreeEntity", entity => entity.SomeProp.ToString(CultureInfo.InvariantCulture));

            _mockStore = TableContextTests.SetupMockTableContext<JsonTableEntry>(Kernel, new Dictionary<string, List<JsonTableEntry>>());


            Kernel.Bind<TestRespositoryBase<JsonTableEntry>>()
                .ToSelf().InTransientScope();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableContextInterface>();
            Kernel.Unbind<TableNameAndIndexProviderServiceInterface>();
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
                                          new TwoEntity() {Id = Guid.NewGuid().ToString(), Prop = "123", PropTwo = aggregateId, AggregateId = aggregateId}, 
                                          new TwoEntity() {Id = Guid.NewGuid().ToString(), Prop = "555", PropTwo = aggregateId, AggregateId = aggregateId }
                                      }
            };

            var repo = Kernel.Get<TestRespositoryBase<JsonTableEntry>>();
            repo.Store(one);
            foreach (var two in one.RelatedEntities)
            {
                repo.Store(two);
            }
            repo.SaveChanges();

            var qs = Kernel.Get<QueryServiceBase<JsonTableEntry>>();
            var ret = qs.FindAggregateEntities<TwoEntity>(aggregateId);

            AssertUtil.Count(2, ret);
            CollectionAssert.AreEquivalent(one.RelatedEntities, ret); 
        }

        [Test]
        public void QueryServiceBaseGetsAllEntityIds()
        {
            _mockStore.Clear();
    
            var repo = Kernel.Get<TestRespositoryBase<JsonTableEntry>>();
            repo.Store(new OneEntity() {Id = Guid.NewGuid().ToString(), Prop = "123", PropTwo = "1" });
            repo.Store(new OneEntity() { Id = Guid.NewGuid().ToString(), Prop = "123", PropTwo = "1" });
            repo.Store(new OneEntity() { Id = Guid.NewGuid().ToString(), Prop = "123", PropTwo = "1" });
            
            repo.SaveChanges();

            var qs = Kernel.Get<QueryServiceBase<JsonTableEntry>>();
            var ret = qs.GetAllIds<OneEntity>();

            AssertUtil.Count(3, ret);
        }

        [Test]
        public void QueryServiceBaseFindsRelatedEntityIds()
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
                                          new TwoEntity() {Id = Guid.NewGuid().ToString(), Prop = "123", PropTwo = aggregateId, AggregateId = aggregateId}, 
                                          new TwoEntity() {Id = Guid.NewGuid().ToString(), Prop = "555", PropTwo = aggregateId, AggregateId = aggregateId }
                                      }
            };

            var repo = Kernel.Get<TestRespositoryBase<JsonTableEntry>>();
            repo.Store(one);
            foreach (var two in one.RelatedEntities)
            {
                repo.Store(two);
            }
            repo.SaveChanges();

            var qs = Kernel.Get<QueryServiceBase<JsonTableEntry>>();
            var ret = qs.FindAggregateEntityIds<TwoEntity>(aggregateId);

            AssertUtil.Count(2, ret);
            CollectionAssert.AreEquivalent(one.RelatedEntities.Select(e => e.Id), ret);
        }
    }
}
