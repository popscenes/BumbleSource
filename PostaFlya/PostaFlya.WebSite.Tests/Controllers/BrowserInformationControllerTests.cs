using System.Web.Mvc;
using System.Web.Script.Serialization;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Controllers;
using PostaFlya.Mocks.Domain.Data;
using Website.Application.Domain.Browser;
using PostaFlya.Domain.Browser;
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
            var browserInfo = Kernel.GetMock<PostaFlyaBrowserInformationInterface>();

            TestRepositoriesNinjectModule.AddBrowsers(Kernel);
            var browser = Kernel.Get<BrowserInterface>(ctx => ctx.Has("postadefaultbrowser"));

            
            browserInfo.Setup(ctx => ctx.Browser)
                .Returns(browser as Domain.Browser.Browser);

            Kernel.Bind<BrowserInformationInterface, PostaFlyaBrowserInformationInterface>()
                .ToConstant(browserInfo.Object);

        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind< PostaFlyaBrowserInformationInterface>();
            Kernel.Unbind<BrowserInformationInterface>();
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
