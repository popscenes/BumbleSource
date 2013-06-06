using System;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using TechTalk.SpecFlow;
using PostaFlya.Controllers;
using PostaFlya.Models.Location;
using PostaFlya.Specification.DynamicBulletinBoard;
using PostaFlya.Specification.Util;
using Website.Application.Domain.Browser;
using Website.Domain.Content;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Command;

namespace PostaFlya.Specification.BrowserSettings
{
    [Binding]
    public class SettingsSteps
    {
        private CommonSteps _common = new CommonSteps();

        

        

        #region My IMAGES STEPS

        [Then(@"i should have a list of my SAVED IMAGES")]
        public void ThenIShouldHaveAListOfMySAVEDIMAGES()
        {
            var browserInformation = SpecUtil.CurrIocKernel.Get<BrowserInformationInterface>();
            var savedImagesApiController = SpecUtil.GetController<MyImagesController>();

            var imageRepo = SpecUtil.CurrIocKernel.Get<GenericRepositoryInterface>();
            var testImage = new Image()
                             {
                                 Id = Guid.NewGuid().ToString(),
                                 Title = "test",
                                 BrowserId = browserInformation.Browser.Id,
                                 Status = ImageStatus.Processing,
                                 Location = new Location(0,0)
                             };
            imageRepo.Store(testImage);

            var imageList = savedImagesApiController.Get(browserInformation.Browser.Id);



            Assert.IsTrue(imageList.Count == 1);
        }


        #endregion

    }
}
