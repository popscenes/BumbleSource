using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using WebSite.Azure.Common.TableStorage;

namespace WebSite.Azure.Common.Tests.TableStorage
{
    [TestFixture]
    public class ClonedTableEntryTests
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

            _mockStore = TableContextTests.SetupMockTableContext<ExtendableTableEntry>(Kernel, new Dictionary<string, List<ExtendableTableEntry>>());
        }

        [FixtureTearDown]
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
            Assert.Count(3, ret);

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
