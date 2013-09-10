using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Test.Common;

namespace Website.Azure.Common.Tests.TableStorage
{
    [TestFixture]
    public class RepositoryBaseTests
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
        public void RepositoryBaseTestStore()
        {
            var id = Guid.NewGuid().ToString();
            var one = new OneEntity()
            {
                Id = id,
                Prop = "Ya",
                PropTwo = "You",
                PropThree = "My property",
                MemberEntity = new ThreeEntity() { SomeProp = 45, MemberEntity = new TwoEntity() { Prop = "ThreeMember", PropTwo = "ThreeMemberTwo" } },
                RelatedEntities = new List<TwoEntity>()
                    {
                        new TwoEntity() { Prop = "123", PropTwo = "333", AggregateId = id}, 
                        new TwoEntity() { Prop = "555", AggregateId = id }
                    }
            };

            var repo = Kernel.Get<TestRespositoryBase<JsonTableEntry>>();
            repo.Store(one);
            foreach (var two in one.RelatedEntities)
            {
                repo.Store(two);
            }
            repo.SaveChanges();

            Assert.That(_mockStore.Count(), Is.EqualTo(2));
            Assert.That(_mockStore["testOneEntity"].Count(), Is.EqualTo(1));
            Assert.That(_mockStore["testTwoEntity"].Count(), Is.EqualTo(2));
            _mockStore.Clear();
        }


        [Test]
        public void RepositoryBaseTestUpdateEntity()
        {
            var id = Guid.NewGuid().ToString();
            var one = new OneEntity()
            {
                Id = id,
                Prop = "Ya",
                PropTwo = "You",
                PropThree = "My property",
                MemberEntity = new ThreeEntity() { SomeProp = 45, MemberEntity = new TwoEntity() { Prop = "ThreeMember", PropTwo = "ThreeMemberTwo" } },
                RelatedEntities = new List<TwoEntity>()
                    {
                        new TwoEntity() { Prop = "123", PropTwo = "333", AggregateId = id}, 
                        new TwoEntity() { Prop = "555", AggregateId = id }
                    }
            };

            var repo = Kernel.Get<TestRespositoryBase<JsonTableEntry>>();
            repo.Store(one);
            foreach (var two in one.RelatedEntities)
            {
                repo.Store(two);
            }
            repo.SaveChanges();



            AssertUtil.Count(2, _mockStore);
            AssertUtil.Count(1, _mockStore["testOneEntity"]);



            repo.UpdateEntity<OneEntity>(one.Id, entity => entity.Prop = "Some Updated Text");
            repo.SaveChanges();

            AssertUtil.Count(2, _mockStore);
            AssertUtil.Count(1, _mockStore["testOneEntity"]);
            AssertUtil.Count(2, _mockStore["testTwoEntity"]);

            Assert.IsTrue(_mockStore["testOneEntity"].Any(entry => entry.GetJson().Contains("Some Updated Text")));

        }


    }

    public class TestRespositoryBase<TableEntryType> 
        : RepositoryBase<TableEntryType>
        where TableEntryType : class, StorageTableEntryInterface, new()
    {
        public TestRespositoryBase(TableContextInterface tableContext
            , TableNameAndIndexProviderServiceInterface nameAndIndexProviderService
            , EventPublishServiceInterface publishService)
            : base(tableContext, nameAndIndexProviderService, publishService)
        {
            MockDeserializationStore = new Dictionary<string, object>();
        }

        public Dictionary<string, object> MockDeserializationStore { get; set; } 

//        protected override StorageAggregate GetEntityForUpdate(Type entity, string id) 
//        {
//            var root = MockDeserializationStore[id];
//            var ret =  new StorageAggregate(root, NameAndPartitionProviderService);
//            ret.LoadAllTableEntriesForUpdate<TableEntryType>(TableContext);
//            return ret;
//        }

        public override void Store<EntityType>(EntityType entity)
        {
            var ent = entity as EntityIdInterface;
            MockDeserializationStore[ent.Id] = entity;
            base.Store(entity);
        }
    }
}
