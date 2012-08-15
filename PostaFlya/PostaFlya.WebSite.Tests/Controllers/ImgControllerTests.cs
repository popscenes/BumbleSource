using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MbUnit.Framework;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Application.Domain.Content.Command;
using PostaFlya.Application.Domain.Tests.Content;
using WebSite.Application.Tests.Content;
using WebSite.Common.ActionResult;
using PostaFlya.Controllers;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using PostaFlya.Domain.Service;
using WebSite.Infrastructure.Command;
using WebSite.Test.Common;
//using WebSite.Mocks.Domain.Data;

namespace PostaFlya.WebSite.Tests.Controllers
{
    [TestFixture]
    public class ImgControllerTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            ImageProcessingTests.FixtureSetUp(Kernel);
            
        }

        [FixtureTearDown]
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
            var image = new Domain.Content.Image() { Id = id, Status = ImageStatus.Processing};
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
