using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using TechTalk.SpecFlow;
using Website.Application.Content;
using PostaFlya.Controllers;
using Website.Infrastructure.Command;
using PostaFlya.Models.Content;
using PostaFlya.Models.Location;
using PostaFlya.Specification.Util;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Website.Domain.Content.Query;
using Website.Domain.Location;
using Website.Mocks.Domain.Data;

namespace PostaFlya.Specification.Content
{
    [Binding]
    public class ImageUploadSteps
    {

        private void UploadAnImage(System.Drawing.Bitmap image)
        {
            //mock the context needed
            var responseMock = new Moq.Mock<HttpResponseBase>();
            var headers = new NameValueCollection();
            responseMock.SetupGet(r => r.Headers).Returns(headers);

            var httpContext = new Moq.Mock<HttpContextBase>();
            httpContext.SetupGet(c => c.Response).Returns(responseMock.Object);

            var contContext = new Moq.Mock<ControllerContext>();
            contContext.Setup(c => c.HttpContext)
                .Returns(httpContext.Object);

            ImageContentRetrieverTestData.SetupContentRetrieverForImage(SpecUtil.CurrIocKernel, image);
            var imgController = SpecUtil.GetController<ImgController>();
            imgController.ControllerContext = contContext.Object;

            var model = new ImageCreateModel(){Location = new LocationModel()};
            var res = imgController.Post(model);
            Assert.IsTrue(res.StatusCode == (int)HttpStatusCode.Created);

            var newImgLoc = imgController.Response.Headers["Location"];
            ScenarioContext.Current["uploadedImage"] = newImgLoc;
        }

        [When("I upload an IMAGE")]
        public void WhenIUploadAnImage()
        {
            UploadAnImage(Properties.Resources.TestImage);
        }

        [Then("the IMAGE should be added to my list of IMAGES")]
        public void ThenTheImageShouldBeAddedToMyListOfImages()
        {
            var img = ScenarioContext.Current["uploadedImage"] as string;
            var browserInformation = SpecUtil.GetCurrBrowser();

            var imageQs = SpecUtil.CurrIocKernel.Get<ImageQueryServiceInterface>();
            var imgs = imageQs.GetByBrowserId<Image>(browserInformation.Browser.Id);
            Assert.IsNotEmpty(imgs);
            Assert.IsNotEmpty(imgs.Where(i => i.Id == img));
        }


        [Then(@"the IMAGE status should be processing")]
        public void ThenTheImageStatusShouldBeProcessing()
        {
            var img = ScenarioContext.Current["uploadedImage"] as string;
            var browserInformation = SpecUtil.GetCurrBrowser();
            var imageQs = SpecUtil.CurrIocKernel.Get<ImageQueryServiceInterface>();
            var retImg = imageQs.FindById<Image>(img);
            Assert.IsNotNull(retImg);
            Assert.IsTrue(retImg.Status == ImageStatus.Processing);
        }

        [When(@"The IMAGE is finished processing")]
        public void WhenTheImageIsFinishedProcessing()
        {
            var img = ScenarioContext.Current["uploadedImage"] as string;
            var imageQs = SpecUtil.CurrIocKernel.Get<ImageQueryServiceInterface>();
            var retImg = imageQs.FindById<Image>(img);
            Assert.IsNotNull(retImg);

           var bus = SpecUtil.CurrIocKernel.Get<CommandBusInterface>();
            bus.Send(new SetImageStatusCommand()
                         {
                             Id = retImg.Id,
                             Status = ImageStatus.Ready
                         });
        }

        [Then(@"the IMAGE status should be ready")]
        public void ThenTheImageStatusShouldBeReady()
        {
            var img = ScenarioContext.Current["uploadedImage"] as string;
            var imageQs = SpecUtil.CurrIocKernel.Get<ImageQueryServiceInterface>();
            var retImg = imageQs.FindById<Image>(img);
            Assert.IsNotNull(retImg);
            Assert.AreEqual(retImg.Status, ImageStatus.Ready);
        }

        [When(@"IMAGE processing fails")]
        public void WhenImageProcessingFails()
        {
            WhenIUploadAnImage();//just pretend the image fails processing
            var img = ScenarioContext.Current["uploadedImage"] as string;
            var imageQs = SpecUtil.CurrIocKernel.Get<ImageQueryServiceInterface>();
            var retImg = imageQs.FindById<Image>(img);
            Assert.IsNotNull(retImg);

            var bus = SpecUtil.CurrIocKernel.Get<CommandBusInterface>();
            bus.Send(new SetImageStatusCommand()
            {
                Id = retImg.Id,
                Status = ImageStatus.Failed
            });

        }
        [Then(@"the IMAGE status should be failed")]
        public void ThenTheImageStatusShouldBeFailed()
        {
            var img = ScenarioContext.Current["uploadedImage"] as string;
            var imageQs = SpecUtil.CurrIocKernel.Get<ImageQueryServiceInterface>();
            var retImg = imageQs.FindById<Image>(img);
            Assert.IsNotNull(retImg);
            Assert.AreEqual(retImg.Status, ImageStatus.Failed);
        }

        [When(@"I upload an IMAGE with Location Exif Data")]
        public void WhenIUploadAnImageWithLocationExifData()
        {
            UploadAnImage(Properties.Resources.ImageWithLocation);
        }

        [Then(@"the IMAGE should have the Same Location as the uploaded IMAGE Exif Data")]
        public void ThenTheImageShouldHaveTheSameLocationAsTheUploadedImageExifData()
        {
            var img = ScenarioContext.Current["uploadedImage"] as string;

            
            var resourceimg =  new ExifImage(Properties.Resources.ImageWithLocation);
            var latitude = resourceimg.GetLatitude();
            var longitude = resourceimg.GetLongitude();
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);

            //in image process
            var bus = SpecUtil.CurrIocKernel.Get<CommandBusInterface>();
            bus.Send(new SetImageMetaDataCommand()
            {
                CommandId = Guid.NewGuid().ToString(),
                Id = img,
                Location = new Location(longitude.Value, latitude.Value)
            });

            var imageQs = SpecUtil.CurrIocKernel.Get<ImageQueryServiceInterface>();
            var retImg = imageQs.FindById<Image>(img);
            Assert.IsNotNull(retImg);
            Assert.AreEqual(retImg.Status, ImageStatus.Ready);

            var exifloc = new Location(longitude.Value, latitude.Value);
            Assert.AreEqual(exifloc, retImg.Location);
        }


    }
}
