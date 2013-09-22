using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Seo;

namespace Website.Application.Tests.Seo
{
    [TestFixture]
    public class GetRenderedHtmlForUrlServiceTests
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
        public void TestPhantomJsLoadsHtml()
        {
            var sub = new GetRenderedHtmlForUrlService();
            var html = sub.GetHtml("http://angularjs.org");
            Assert.That(html, Contains.Substring("<html "));

        }

        [Test]
        public void TestPhantomJsLoadsHtmlWaitsForStatus()
        {
            var sub = new GetRenderedHtmlForUrlService();
            var html = sub.GetHtml("http://angularjs.org", true, 5000);
            Assert.That(html, Is.EqualTo(""));

        }
    }
}
