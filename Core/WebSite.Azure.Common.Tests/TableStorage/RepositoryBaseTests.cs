using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Domain;
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
            Kernel.Rebind<TableNameAndPartitionProviderServiceInterface>()
                .To<TableNameAndPartitionProviderService>()
                .InSingletonScope();

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
            tableNameAndPartitionProviderService.Add<OneEntity>(0, "testOneEntity", entity => entity.Prop);
            tableNameAndPartitionProviderService.Add<OneEntity>(1, "testOneEntity", entity => entity.PropTwo, entity => entity.Prop);
            tableNameAndPartitionProviderService.Add<OneEntity>(2, "testOneEntity", entity => entity.Prop + entity.PropTwo, entity => entity.PropTwo);

            tableNameAndPartitionProviderService.Add<TwoEntity>(0, "testTwoEntity", entity => entity.Prop);
            tableNameAndPartitionProviderService.Add<TwoEntity>(1, "testTwoEntity", entity => entity.PropTwo, entity => entity.Prop);

            tableNameAndPartitionProviderService.Add<ThreeEntity>(0, "testThreeEntity", entity => entity.SomeProp.ToString(CultureInfo.InvariantCulture));

            _mockStore = TableContextTests.SetupMockTableContext<JsonTableEntry>(Kernel, new Dictionary<string, List<JsonTableEntry>>());

            Kernel.Bind<TestRespositoryBase<JsonTableEntry>>()
                .ToSelf().InTransientScope();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableContextInterface>();
            Kernel.Unbind<TableNameAndPartitionProviderServiceInterface>();
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
                RelatedEntities = new List<TwoEntity>() { new TwoEntity() { Prop = "123", PropTwo = "333" }, new TwoEntity() { Prop = "555" } }
            };

            var repo = Kernel.Get<TestRespositoryBase<JsonTableEntry>>();
            repo.Store(one);
            repo.SaveChanges();

            Assert.That(_mockStore.Count(), Is.EqualTo(3));
            Assert.That(_mockStore["testOneEntity"].Count(), Is.EqualTo(3));
            Assert.That(_mockStore["testTwoEntity"].Count(), Is.EqualTo(6));
            Assert.That(_mockStore["testThreeEntity"].Count(), Is.EqualTo(1));
            _mockStore.Clear();
        }


        [Test]
        public void RepositoryBaseTestUpdateEntity()
        {
            var one = new OneEntity()
            {
                Id = Guid.NewGuid().ToString(),
                Prop = "Ya",
                PropTwo = "You",
                PropThree = "My property",
                MemberEntity = new ThreeEntity() { SomeProp = 45, MemberEntity = new TwoEntity() { Prop = "ThreeMember", PropTwo = "ThreeMemberTwo" } },
                RelatedEntities = new List<TwoEntity>() { new TwoEntity() { Prop = "123", PropTwo = "333" }, new TwoEntity() { Prop = "555" } }
            };

            var repo = Kernel.Get<TestRespositoryBase<JsonTableEntry>>();
            repo.Store(one);
            repo.SaveChanges();



            AssertUtil.Count(3, _mockStore);
            AssertUtil.Count(3, _mockStore["testOneEntity"]);
            AssertUtil.Count(6, _mockStore["testTwoEntity"]);
            AssertUtil.Count(1, _mockStore["testThreeEntity"]);


            repo.UpdateEntity<OneEntity>(one.Id, entity => entity.Prop = "Some Updated Text");
            repo.SaveChanges();

            AssertUtil.Count(3, _mockStore);
            AssertUtil.Count(3, _mockStore["testOneEntity"]);
            AssertUtil.Count(6, _mockStore["testTwoEntity"]);
            AssertUtil.Count(1, _mockStore["testThreeEntity"]);

            Assert.IsTrue(_mockStore["testOneEntity"].Any(entry => entry.GetJson().Contains("Some Updated Text")));

        }
    }

    public class TestRespositoryBase<TableEntryType> 
        : RepositoryBase<TableEntryType>
        where TableEntryType : class, StorageTableEntryInterface, new()
    {
        public TestRespositoryBase(TableContextInterface tableContext
            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService) 
            : base(tableContext, nameAndPartitionProviderService, null)
        {
            MockDeserializationStore = new Dictionary<string, object>();
        }

        public Dictionary<string, object> MockDeserializationStore { get; set; } 

        protected override StorageAggregate GetEntityForUpdate(Type entity, string id) 
        {
            var root = MockDeserializationStore[id];
            var ret =  new StorageAggregate(root, NameAndPartitionProviderService);
            ret.LoadAllTableEntriesForUpdate<TableEntryType>(TableContext);
            return ret;
        }

        public override void Store<EntityType>(EntityType entity)
        {
            var ent = entity as EntityIdInterface;
            MockDeserializationStore[ent.Id] = entity;
            base.Store(entity);
        }
    }
}
