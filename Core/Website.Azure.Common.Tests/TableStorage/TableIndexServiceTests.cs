using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;

namespace Website.Azure.Common.Tests.TableStorage
{
    [TestFixture("dev")]
    //[TestFixture("real")]
    public class TableIndexServiceTests
    {
        static MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public TableIndexServiceTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        }

        Dictionary<string, List<JsonTableEntry>> _mockStore;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<TableContextInterface>()
                .To<TableContext>();

            Kernel.Rebind<TableNameAndIndexProviderServiceInterface>()
                .To<TableNameAndIndexProviderService>()
                .InSingletonScope();

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();
            tableNameAndPartitionProviderService.Add<OneEntity>("testOneEntity", entity => entity.Prop);
            tableNameAndPartitionProviderService.AddIndex("testOneIndex", StandardIndexSelectors.FriendlyIdIndex, StandardIndexSelectors.FriendlyIdSelector<OneEntity>());

            var context = Kernel.Get<TableContextInterface>();

            foreach (var tableName in tableNameAndPartitionProviderService.GetAllTableNames())
            {
                context.InitTable<JsonTableEntry>(tableName);
            }

        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndIndexProviderServiceInterface>();
            Kernel.Unbind<TableContextInterface>();
            AzureEnv.UseRealStorage = false;
        }

        [SetUp]
        public void BeforeTest()
        {
            var context = Kernel.Get<TableContextInterface>();
            context.Delete<JsonTableEntry>("testOneEntity", null);
            context.Delete<JsonTableEntry>("testOneIndex", null);
            context.SaveChanges(); 
        }

        [Test]
        public void TableIndexServiceCreatesIndexRecordsForEntities()
        {
            var newEntity = new OneEntity {Id = "1234", Prop = "12345", FriendlyId = "IamFriendly"};
            var indexService = Kernel.Get<TableIndexService>();
            indexService.UpdateEntityIndexes(newEntity);

            var ctx = Kernel.Get<TableContextInterface>();
            var all = ctx.PerformQuery<JsonTableEntry>("testOneIndex");
            Assert.That(all.Count(), Is.EqualTo(1));
            Assert.True(all.Any(entry => entry.PartitionKey.Contains("IamFriendly".ToStorageKeySection())));
        }

        [Test]
        public void TableIndexServiceDeletesOldIndexRecordsForEntities()
        {
            TableIndexServiceCreatesIndexRecordsForEntities();

            var newEntity = new OneEntity { Id = "1234", Prop = "12345", FriendlyId = "IamFriendly" };
            var indexService = Kernel.Get<TableIndexService>();
            indexService.UpdateEntityIndexes(newEntity, true);

            var ctx = Kernel.Get<TableContextInterface>();
            var all = ctx.PerformQuery<JsonTableEntry>("testOneIndex");
            Assert.That(all.Count(), Is.EqualTo(0));

        }

    }
}
