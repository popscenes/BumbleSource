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
using Website.Infrastructure.Domain;
using Website.Azure.Common.TableStorage;

namespace Website.Azure.Common.Tests.TableStorage
{
    [TestFixture]
    public class JsonRepositoryTests
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

            tableNameAndPartitionProviderService.Add<TwoEntity>(0, "testTwoEntity", entity => entity.Prop);
            tableNameAndPartitionProviderService.Add<TwoEntity>(1, "testTwoEntity", entity => entity.PropTwo, entity => entity.Prop);

            tableNameAndPartitionProviderService.Add<ThreeEntity>(0, "testThreeEntity", entity => entity.SomeProp.ToString(CultureInfo.InvariantCulture));

            _mockStore = TableContextTests.SetupMockTableContext<JsonTableEntry>(Kernel, new Dictionary<string, List<JsonTableEntry>>());

            Kernel.Bind<JsonRepository>()
                .ToSelf().InTransientScope();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableContextInterface>();
            Kernel.Unbind<TableNameAndPartitionProviderServiceInterface>();
            Kernel.Unbind<JsonRepository>();
        }

        [Test]
        public void JsonRepositoryUpdatesEntity()
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

            var repo = Kernel.Get<JsonRepository>();
            repo.Store(one);
            repo.SaveChanges();



            Assert.Count(3, _mockStore);
            Assert.Count(3, _mockStore["testOneEntity"]);
            Assert.Count(6, _mockStore["testTwoEntity"]);
            Assert.Count(1, _mockStore["testThreeEntity"]);


            repo.UpdateEntity<OneEntity>(one.Id, entity => entity.Prop = "Some Updated Text");
            repo.SaveChanges();

            Assert.Count(3, _mockStore);
            Assert.Count(3, _mockStore["testOneEntity"]);
            Assert.Count(6, _mockStore["testTwoEntity"]);
            Assert.Count(1, _mockStore["testThreeEntity"]);

            Assert.IsTrue(_mockStore["testOneEntity"].Any(entry => entry.GetJson().Contains("Some Updated Text")));
        }


    }
}
