using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Controllers;
using PostaFlya.Models.Browser;
using Microsoft.CSharp;
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

        [FixtureSetUp]
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

        [FixtureTearDown]
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
            //Assert.Count(2, browserInfoModel.SavedLocations);
            //Assert.Count(2, browserInfoModel.SavedTags);

        }
    }
}
