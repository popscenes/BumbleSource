using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Infrastructure.Domain;
using Website.Azure.Common.TableStorage;

namespace Website.Azure.Common.Tests.TableStorage
{
    [TestFixture]
    public class AggregateEntityTableEntryCollectionTests
    {
        static MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        Dictionary<string, List<ExtendableTableEntry>> _mockStore;

        [FixtureSetUp]
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

            _mockStore = TableContextTests.SetupMockTableContext<ExtendableTableEntry>(Kernel, new Dictionary<string, List<ExtendableTableEntry>>());
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableContextInterface>();
            Kernel.Unbind<TableNameAndPartitionProviderServiceInterface>();
        }



        [Test]
        public void AggregateEntityTableEntryCollectionCreatesEntriesForAllMemberEntities()
        {
            var one = new OneEntity()
            {
                Prop = "Ya",
                PropTwo = "You",
                PropThree = "My property",
                MemberEntity = new ThreeEntity(){SomeProp = 45, MemberEntity = new TwoEntity(){Prop = "ThreeMember", PropTwo = "ThreeMemberTwo"}},
                RelatedEntities = new List<TwoEntity>(){new TwoEntity(){Prop = "123", PropTwo = "333"}, new TwoEntity(){Prop = "555"}}
            };

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            var entityTableEntryCollection = new AggregateEntityTableEntryCollection();
            entityTableEntryCollection.UpdateFrom(one, tableNameAndPartitionProviderService);

            var ret = entityTableEntryCollection.ClonedTableEntries;
            Assert.Count(5, ret);

            one.RelatedEntities.Add(new TwoEntity(){Prop = "Added Prop"});
            entityTableEntryCollection.UpdateFrom(one, tableNameAndPartitionProviderService);
            ret = entityTableEntryCollection.ClonedTableEntries;
            Assert.Count(6, ret);
        }
    }
}
