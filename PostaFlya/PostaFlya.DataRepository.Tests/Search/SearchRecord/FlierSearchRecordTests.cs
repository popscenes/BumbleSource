using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using PostaFlya.DataRepository.Search.SearchRecord;
using PostaFlya.Mocks.Domain.Data;
using Website.Domain.Location;

namespace PostaFlya.DataRepository.Tests.Search.SearchRecord
{
    [TestFixture]
    public class FlierSearchRecordTests
    {

        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
        }

        [Test]
        public void ToSearchRecordsCreatesARecordForEachUniqueLocationShardIdTest()
        {
            //create a flier that will result in different integral long lat when buffer expanded
            var flier = FlierTestData.GetOne(Kernel, new Location(110, 80));
            flier.LocationRadius = 2;

            var ret = flier.ToSearchRecords();
            Assert.That(ret.Count(), Is.EqualTo(4));
        }

        [Test]
        public void ToSearchRecordsCreatesOneRecordWhenLocationIsntExpandedTest()
        {
            //create a flier that will result in different integral long lat when buffer expanded
            var flier = FlierTestData.GetOne(Kernel, new Location(110, 80));
            flier.LocationRadius = 0;

            var ret = flier.ToSearchRecords();
            Assert.That(ret.Count(), Is.EqualTo(1));
        }
    }
}
