using System.Web.Mvc;
using System.Web.Script.Serialization;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Controllers;
using Website.Application.Domain.Browser;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Domain.Tag;


namespace PostaFlya.Website.Tests.Controllers
{
    [TestFixture]
    public class BrowserInformationControllerTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var browserInfo = Kernel.GetMock<BrowserInformationInterface>();

            var browser =  Kernel.Get<BrowserInterface>(ctx => ctx.Has("ststestbrowser"));

            browser.SavedTags.Add(Kernel.Get<Tags>(ctx => ctx.Has("defaulttags2")));
            browser.SavedTags.Add(Kernel.Get<Tags>(ctx => ctx.Has("someothertags")));
            browser.SavedLocations.Add(new Location(0, 0));
            browser.SavedLocations.Add(new Location(10, 20));
            browserInfo.Setup(ctx => ctx.Browser).Returns(browser);

            
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            
        }

        [Test]
        public void TestBrowserInformationControllerGet()
        {
            var browserInfoCotroller = Kernel.Get<BrowserInformationController>();
            var browserInfoModelJSON = browserInfoCotroller.BrowserInfo() as ViewResult;

            

            var serializer = new JavaScriptSerializer();
            //var browserInfoModel = serializer.Deserialize<CurrentBrowserModel>(browserInfoModelJSON.ViewBag.BrowserInfoJson);
            //AssertUtil.Count(2, browserInfoModel.SavedLocations);
            //AssertUtil.Count(2, browserInfoModel.SavedTags);

        }
    }
}
