using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Command;
using Website.Test.Common;

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

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<TableNameAndPartitionProviderServiceInterface>()
                .To<TableNameAndPartitionProviderService>()
                .InSingletonScope();

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
            tableNameAndPartitionProviderService.Add<OneEntity>("testOneEntity", entity => entity.Id);

            tableNameAndPartitionProviderService.Add<TwoEntity>("testTwoEntity", entity => entity.Prop);

            tableNameAndPartitionProviderService.Add<ThreeEntity>("testThreeEntity", entity => entity.SomeProp.ToString(CultureInfo.InvariantCulture));

            _mockStore = TableContextTests.SetupMockTableContext<JsonTableEntry>(Kernel, new Dictionary<string, List<JsonTableEntry>>());

            Kernel.Bind<JsonRepository>()
                .ToSelf().InTransientScope();
        }

        [TestFixtureTearDown]
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
}
