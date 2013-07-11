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

    }
}
