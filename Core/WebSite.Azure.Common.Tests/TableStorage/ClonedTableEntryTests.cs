using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Azure.Common.TableStorage;

namespace Website.Azure.Common.Tests.TableStorage
{
    [TestFixture]
    public class ClonedTableEntryTests
    {
        static MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        Dictionary<string, List<ExtendableTableEntry>> _mockStore;

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

            _mockStore = TableContextTests.SetupMockTableContext<ExtendableTableEntry>(Kernel, new Dictionary<string, List<ExtendableTableEntry>>());
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableContextInterface>();
            Kernel.Unbind<TableNameAndPartitionProviderServiceInterface>();
        }


        [Test]
        public void ClonedTableEntryPopulatePartitionClonesForNewEntity()
        {

            var test = Kernel.Get<ClonedTableEntry>();

            var one = new OneEntity()
                {
                    Prop = "Ya",
                    PropTwo = "You"
                };

            test.PopulatePartitionClones<ExtendableTableEntry>(one, Kernel.Get<TableContextInterface>());

            var ret = test.Entries.Select(kv => kv.Value)
                .OfType<ExtendableTableEntry>().ToList();

            Assert.That(ret.Count(), Is.EqualTo(3));

            var nameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
            foreach (var partition in nameAndPartitionProviderService.GetPartitionIdentifiers<OneEntity>())
            {
                var partKeyFunc = nameAndPartitionProviderService.GetPartitionKeyFunc<OneEntity>(partition);
                var rowKeyFunc = nameAndPartitionProviderService.GetRowKeyFunc<OneEntity>(partition);

                Assert.IsTrue(ret.Any(e => e.PartitionKey == partKeyFunc(one) && e.RowKey == rowKeyFunc(one)));
            }
        }
    }


}
