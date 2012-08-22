using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public class StorageAggregateTests
    {
        static MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        Dictionary<string, List<ExtendableTableEntry>> _mockStore;

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Unbind<TableContextInterface>();
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
        public void StorageAggregateGetTableEntriesReturnsAllTableEntriesForAggregateRoot()
        {
            var one = new OneEntity()//3 entries
            {
                Prop = "Ya",
                PropTwo = "You",
                PropThree = "My property",
                MemberEntity = new ThreeEntity()//1 entry
                                   {
                                       SomeProp = 45,
                                       MemberEntity = new TwoEntity() //2 entries
                                       { Prop = "ThreeMember", PropTwo = "ThreeMemberTwo" }
                                   },
                RelatedEntities = new List<TwoEntity>() 
                {   new TwoEntity() //2 entries
                        { Prop = "123", PropTwo = "333" }, 
                    new TwoEntity() //2 entries
                        { Prop = "555" } 
                }
            };

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            var testob = new StorageAggregate(one, tableNameAndPartitionProviderService);
            var tabCtx = Kernel.Get<TableContextInterface>();
            var entries = testob.GetTableEntries<ExtendableTableEntry>(tabCtx);
            Assert.Count(10, entries);

            one.RelatedEntities.Add(new TwoEntity(){Prop = "777", PropTwo = "888"});
            one.PropThree = "My property Has Changed";
            entries = testob.GetTableEntries<ExtendableTableEntry>(tabCtx);
            Assert.IsTrue(entries.Any(e => Equals("My property Has Changed", e.Entry["PropThree"])));
            Assert.Count(12, entries);
        }
    }
}
