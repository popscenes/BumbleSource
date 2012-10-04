using System;
using System.IO;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Common.ActionResult;
using PostaFlya.Controllers;
using Website.Application.Domain.Tests.Content;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Website.Test.Common;
//using Website.Mocks.Domain.Data;

namespace PostaFlya.Website.Tests.Controllers
{
    [TestFixture]
    public class ImgControllerTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            ImageProcessingTests.FixtureSetUp(Kernel);
            
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            ImageProcessingTests.FixtureTearDown(Kernel);
        }


//going straight to blob storage or CDN now
//        [Test]
//        public void TestImgControllerReturnsAllImages()
//        {
//            ImageProcessingTests.AssertWithTestImage(Kernel, AssertImgControllerReturnsImage);
//            var imgController = Kernel.Get<ImgController>();
//        }

//        private void AssertImgControllerReturnsImage(Guid imageId, Dictionary<string, byte[]> storage)
//        {
//            var imgController = Kernel.Get<ImgController>();
//            var contContext = new Moq.Mock<ControllerContext>();
//            var cacheMock = new Moq.Mock<HttpCachePolicyBase>();
//            cacheMock.SetupGet(c => c.VaryByParams)
//                .Returns(new HttpCacheVaryByParams());
//            contContext.Setup(c => c.HttpContext.Response.Cache)
//                .Returns(cacheMock.Object);
//            imgController.ControllerContext = contContext.Object;
//            
//
//            foreach (var res in (from ThumbOrientation orientation in
//                                     Enum.GetValues(typeof (ThumbOrientation))
//                                 from ThumbSize size in
//                                     Enum.GetValues(typeof (ThumbSize))
//                                 select new {orientation, size}).Select(orientationSize => ImageProcessCommandHandler.GetIdExtensionForThumb(
//                                     orientationSize.orientation,
//                                     orientationSize.size))
//                                     .Select(view => imgController.Get(imageId.ToString(), view)))
//            {
//                Assert.IsNotEmpty(GetFileResData(res));
//            }
//        }

        private byte[] GetFileResData(ActionResult actionResult) 
        {
            var filres = actionResult as WriteToStreamFileResult;
            if(filres != null)
            {
                using (var ms = new MemoryStream())
                {
                    filres.WriteAction(ms);
                    return ms.ToArray();
                }
            }

            var filcontentres = actionResult as FileContentResult;
            if (filcontentres != null)
            {
                return filcontentres.FileContents;
            }

            return null;
        }

        
        [Test]
        public void TestImgControllerReturnsNotFoundImageForBogusReq()
        {
            var imgController = Kernel.Get<ImgController>();

            //going straight to blob storage or CDN now
//            var res = imgController.Get("blahblahblah");
//            var fileContents = GetFileResData(res);
//
//            Assert.IsNull(fileContents);
//            Assert.AreEqual(imgController.ControllerContext.HttpContext.Response.StatusCode, 404);

            var res = imgController.GetError("blahblahblah");
            var fileContents = GetFileResData(res);

            Assert.IsNotEmpty(fileContents);
            Assert.AreEqual(fileContents, imgController.GetNotFoundData());
        }

        [Test]
        public void TestImgControllerReturnsFailedImageForInvalidProcess()
        {
            var imgController = Kernel.Get<ImgController>();
            ControllerContextMock.FakeControllerContext(Kernel, imgController);

            var id = ImageProcessingTests.ImageProcessCommandHandlerSetsImageStatusToFailedOnInvalidProcessing(Kernel);

            //going straight to blob storage or CDN now
//            var res = imgController.Get(id);
//
//            var fileContents = GetFileResData(res);
//            Assert.IsNull(fileContents);
//            Assert.AreEqual(imgController.ControllerContext.HttpContext.Response.StatusCode, 404);

            var res = imgController.GetError(id);
            var fileContents = GetFileResData(res);

            Assert.AreEqual(fileContents, imgController.GetFailedProcessingData());
        }

        [Test]
        public void TestImgControllerReturnsProcessingImageForImageStillProcessing()
        {
            var id = Guid.NewGuid().ToString();
            var image = new Image() { Id = id, Status = ImageStatus.Processing};
            Kernel.Get<ImageRepositoryInterface>().Store(image);

            var imgController = Kernel.Get<ImgController>();
            ControllerContextMock.FakeControllerContext(Kernel, imgController);

            //going straight to blob storage or CDN now
//            var res = imgController.Get(id);
//            var fileContents = GetFileResData(res);
//            Assert.IsNull(fileContents);
//            Assert.AreEqual(imgController.ControllerContext.HttpContext.Response.StatusCode, 404);

            var res = imgController.GetError(id);
            var fileContents = GetFileResData(res);

            Assert.AreEqual(fileContents, imgController.GetStillProcessingData());
        }
    }
}
