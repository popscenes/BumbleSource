using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Content;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Test.Common;
using Website.Application.Domain.Content;
using Website.Application.Domain.Content.Command;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Website.Domain.Service;
using Image = System.Drawing.Image;

namespace Website.Application.Domain.Tests.Content
{
    [TestFixture]
    public class ImageProcessingTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public static void FixtureSetUp(MoqMockingKernel kernel)
        {
            kernel.Bind<ContentStorageServiceInterface>().To<ImageProcessContentStorageService>();
            kernel.Bind<CommandBusInterface>().To<DefaultCommandBus>();
            kernel.Bind<CommandHandlerInterface<ImageProcessCommand>>().To<ImageProcessCommandHandler>();
            kernel.Bind<CommandHandlerInterface<ImageProcessSetMetaDataCommand>>().To<ImageProcessSetMetaDataCommandHandler>();
            HttpContextMock.FakeHttpContext(kernel);
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            FixtureSetUp(Kernel);
        }

        public static void FixtureTearDown(MoqMockingKernel kernel)
        {
            kernel.Unbind<ContentStorageServiceInterface>();
            kernel.Unbind<CommandBusInterface>();
            kernel.Unbind<CommandHandlerInterface<ImageProcessCommand>>();
            kernel.Unbind<CommandHandlerInterface<ImageProcessSetMetaDataCommand>>();
            //kernel.Unbind<ImageRepositoryInterface>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            FixtureTearDown(Kernel);
        }

        //for use with other tests that just need one image
        public static void AssertWithTestImage(MoqMockingKernel kernel, Action<Guid, Dictionary<string, byte[]>> assertions)
        {
            System.Drawing.Bitmap bitmap = Website.Application.Tests.Properties.Resources.TestLongImage;
            AssertImage(kernel, bitmap, assertions);
        }

        //image processing tests
        [Test]
        public void ProcessImageCommandHandlerShouldLimitWidthOfImage()
        {
            System.Drawing.Bitmap bitmap = Website.Application.Tests.Properties.Resources.TestWideImage;
            AssertImage(bitmap, TestImageProcessedIsSmallerThanMax);
        }

        [Test]
        public void ProcessImageCommandHandlerShouldLimitHeightOfImage()
        {
            System.Drawing.Bitmap bitmap = Website.Application.Tests.Properties.Resources.TestLongImage;
            AssertImage(bitmap, TestImageProcessedIsSmallerThanMax);

        }

        private void AssertImage(Bitmap bitmap, Action<Guid, Dictionary<string, byte[]>> assertions)
        {
            AssertImage(Kernel, bitmap, assertions);
        }

        private static void AssertImage(MoqMockingKernel kernel, Bitmap bitmap, Action<Guid, Dictionary<string, byte[]>> assertions)
        {
            var storage = new Dictionary<string, byte[]>();
            var mockstore = kernel.GetMock<BlobStorageInterface>();
            mockstore.Setup(s => s.SetBlob(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<BlobProperties>()))
                .Callback<string, byte[], BlobProperties>((s, b, p) =>
                                              {
                                                  if (storage.ContainsKey(s))
                                                      storage.Remove(s);
                                                  storage.Add(s, b);
                                              });
            mockstore.Setup(s => s.GetBlob(It.IsAny<string>()))
                .Returns<string>(s =>
                {
                    byte[] ret; storage.TryGetValue(s, out ret); return ret;
                });

                                         
            byte[] data = null;
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                data = ms.ToArray();
            }

            var command = new CreateImageCommand()
                              {
                                  BrowserId = Guid.NewGuid().ToString(),
                                  ExternalId = "123|facebook",
                                  Title = "Yoyoyoyo",
                                  Content = new Website.Domain.Content.Content()
                                                {
                                                    Type = Website.Domain.Content.Content.ContentType.Image,
                                                    Data = data
                                                }
                              };
            
            //simulate and image upload
            var imageInterface = kernel.Get<CommandBusInterface>().Send(command) as ImageInterface;

            Assert.IsNotNull(imageInterface);

            //test the state was initially processing
            Assert.AreEqual(ImageStatus.Processing, imageInterface.Status);

            var imageQueryService = ResolutionExtensions.Get<GenericQueryServiceInterface>(kernel);
            

            //test the state is ready
            Assert.AreEqual(ImageStatus.Ready, imageQueryService.FindById<Website.Domain.Content.Image>(imageInterface.Id).Status);

            assertions(new Guid(imageInterface.Id), storage);

            var qc = kernel.Get<QueryChannelInterface>();

            //command.CommandId = Guid.NewGuid().ToString();
            
            //kernel.Get<CommandBusInterface>().Send(command);
            var browserImages = qc.Query(new GetByBrowserIdQuery<Website.Domain.Content.Image>() { BrowserId = command.BrowserId },
                                         new List<Website.Domain.Content.Image>());

            //make sure only 1 image per external id
            AssertUtil.Count(1, browserImages);
            kernel.Unbind<BlobStorageInterface>();
        }

        private static void TestImageProcessedIsSmallerThanMax(Guid guid, Dictionary<string, byte[]> dictionary)
        {
            var data = dictionary[guid.ToString() + ImageUtil.GetIdFileExtension()];
            using (var ms = new MemoryStream(data))
            {
                var conv = Image.FromStream(ms);
                Assert.IsTrue(conv.Height <= ImageProcessCommandHandler.MaxHeight);
                Assert.IsTrue(conv.Width <= ImageProcessCommandHandler.MaxWidth);
            }
        }

        [Test]
        public void ProcessImageCommandHandlerShouldLimitWidthAspectRatio()
        {
            System.Drawing.Bitmap bitmap = Website.Application.Tests.Properties.Resources.TestWideWideImage;
            AssertImage(bitmap, TestImageWidthAspectRatioIsLessThanMaxAspectRatio);
        }

        [Test]
        public void ProcessImageCommandHandlerShouldLimitHeightAspectRatio()
        {
            System.Drawing.Bitmap bitmap = Website.Application.Tests.Properties.Resources.TestLongLongImage;
            AssertImage(bitmap, TestImageHeightAspectRatioIsLessThanMaxAspectRatio);
        }

        private static void TestImageWidthAspectRatioIsLessThanMaxAspectRatio(Guid guid, Dictionary<string, byte[]> dictionary)
        {

            var data = dictionary[guid.ToString() + ImageUtil.GetIdFileExtension()];
            using (var ms = new MemoryStream(data))
            {
                var conv = Image.FromStream(ms);
                var aspect = (double)conv.Width / conv.Height;
                Assert.IsTrue(aspect <= ImageProcessCommandHandler.MaxAspectRatio);
            }
        }

        private static void TestImageHeightAspectRatioIsLessThanMaxAspectRatio(Guid guid, Dictionary<string, byte[]> dictionary)
        {
            var data = dictionary[guid.ToString() + ImageUtil.GetIdFileExtension()];
            using (var ms = new MemoryStream(data))
            {
                var conv = Image.FromStream(ms);
                var aspect = (double) conv.Height/conv.Width;
                Assert.IsTrue(aspect <= ImageProcessCommandHandler.MaxAspectRatio);
            }
        }

        [Test]
        public void ProcessImageCommandHandlerShouldCreateTwoThumbsByWidth()
        {
            System.Drawing.Bitmap bitmap = Website.Application.Tests.Properties.Resources.TestWideImage;
            AssertImage(bitmap, TestForThumbnailsOfWidth);
            bitmap = Website.Application.Tests.Properties.Resources.TestLongImage;
            AssertImage(bitmap, TestForThumbnailsOfWidth);
            bitmap = Website.Application.Tests.Properties.Resources.TestWideWideImage;
            AssertImage(bitmap, TestForThumbnailsOfWidth);
            bitmap = Website.Application.Tests.Properties.Resources.TestLongLongImage;
            AssertImage(bitmap, TestForThumbnailsOfWidth);
            bitmap = Website.Application.Tests.Properties.Resources.TestWideSmallImage;
            AssertImage(bitmap, TestForThumbnailsOfWidth);
        }

        private static void TestForThumbnailsOfWidth(Guid id, Dictionary<string, byte[]> storage)
        {
            foreach (ThumbSize size in Enum.GetValues(typeof(ThumbSize)))
            {
                var data = storage[id.ToString() +
                ImageUtil.GetIdFileExtension(
                    ThumbOrientation.Horizontal,
                    size
                )];
                Assert.IsNotNull(data);
                using (var ms = new MemoryStream(data))
                {
                    var conv = Image.FromStream(ms);
                    Assert.AreEqual(conv.Width, (int)size);
                }           
            }
        }

        [Test]
        public void ProcessImageCommandHandlerShouldCreateThumbsByOriginal()
        {
            System.Drawing.Bitmap bitmap = null;
            Action<Guid, Dictionary<string, byte[]>> assertion =
                (guid, byteses) =>
                    {
                        TestForThumbnailsOfOriginal(bitmap, guid, byteses);
                    };
            bitmap = Website.Application.Tests.Properties.Resources.TestWideImage;
            AssertImage(bitmap, assertion);
            bitmap = Website.Application.Tests.Properties.Resources.TestLongImage;
            AssertImage(bitmap, assertion);
            bitmap = Website.Application.Tests.Properties.Resources.TestWideWideImage;
            AssertImage(bitmap, assertion);
            bitmap = Website.Application.Tests.Properties.Resources.TestLongLongImage;
            AssertImage(bitmap, assertion);
            bitmap = Website.Application.Tests.Properties.Resources.TestWideSmallImage;
            AssertImage(bitmap, assertion);
        }

        private static void TestForThumbnailsOfOriginal(Bitmap orig, Guid id, Dictionary<string, byte[]> storage)
        {
            foreach (ThumbSize size in Enum.GetValues(typeof(ThumbSize)))
            {
                var data = storage[id.ToString() +
                ImageUtil.GetIdFileExtension(
                    ThumbOrientation.Original,
                    size
                )];
                Assert.IsNotNull(data);
                using (var ms = new MemoryStream(data))
                {
                    var conv = Image.FromStream(ms);
                    if(orig.Width > orig.Height)
                        Assert.AreEqual(conv.Width, (int)size);
                    else
                        Assert.AreEqual(conv.Height, (int)size);                        
                }
            }
        }



//        [Test]
//        public void ProcessImageCommandHandlerShouldCreateTwoThumbsByLength()
//        {
//            System.Drawing.Bitmap bitmap = Website.Application.Tests.Properties.Resources.TestWideImage;
//            AssertImage(bitmap, TestForThumbnailsOfHeight);
//            bitmap = Website.Application.Tests.Properties.Resources.TestLongImage;
//            AssertImage(bitmap, TestForThumbnailsOfHeight);
//            bitmap = Website.Application.Tests.Properties.Resources.TestWideWideImage;
//            AssertImage(bitmap, TestForThumbnailsOfHeight);
//            bitmap = Website.Application.Tests.Properties.Resources.TestLongLongImage;
//            AssertImage(bitmap, TestForThumbnailsOfHeight);
//        }

//        private void TestForThumbnailsOfHeight(Guid id, Dictionary<string, byte[]> storage)
//        {
//            foreach (ThumbSize size in Enum.GetValues(typeof(ThumbSize)))
//            {
//                var data = storage[id.ToString() +
//                ImageUtil.GetIdFileExtension(
//                    ThumbOrientation.Vertical,
//                    size
//                )];
//                Assert.IsNotNull(data);
//                using (var ms = new MemoryStream(data))
//                {
//                    var conv = Image.FromStream(ms);
//                    Assert.AreEqual(conv.Height, (int)size);
//                }
//            }
//        }

        [Test]
        public void ProcessImageCommandHandlerShouldCreateThumbsBySource()
        {
            System.Drawing.Bitmap bitmap = Website.Application.Tests.Properties.Resources.TestWideImage;
            AssertImage(bitmap, TestForThumbnailOffSource);
            bitmap = Website.Application.Tests.Properties.Resources.TestLongImage;
            AssertImage(bitmap, TestForThumbnailOffSource);
            bitmap = Website.Application.Tests.Properties.Resources.TestWideWideImage;
            AssertImage(bitmap, TestForThumbnailOffSource);
            bitmap = Website.Application.Tests.Properties.Resources.TestLongLongImage;
            AssertImage(bitmap, TestForThumbnailOffSource);

        }

        private void TestForThumbnailOffSource(Guid id, Dictionary<string, byte[]> storage)
        {

            var data = storage[id.ToString() +
            ImageUtil.GetIdFileExtension(
                ThumbOrientation.Original,
                ThumbSize.S150
            )];

            Assert.IsNotNull(data);
            using (var ms = new MemoryStream(data))
            {
                using(var conv = Image.FromStream(ms))
                {
                    Assert.IsTrue(
                               conv.Height == (int)ThumbSize.S150
                            || conv.Width == (int)ThumbSize.S150
                    );
                }
            }
        }

        [Test]
        public void ImageProcessCommandHandlerSetsImageStatusToFailedOnInvalidProcessingTest()
        {
            ImageProcessCommandHandlerSetsImageStatusToFailedOnInvalidProcessing(Kernel);
        }

        public string ImageProcessCommandHandlerSetsImageStatusToFailedOnInvalidProcessing()
        {
            return ImageProcessCommandHandlerSetsImageStatusToFailedOnInvalidProcessing(Kernel);
        }

        public static string ImageProcessCommandHandlerSetsImageStatusToFailedOnInvalidProcessing(MoqMockingKernel kernel)
        {
            //simulate and image upload
            var imageInterface = kernel.Get<CommandBusInterface>().Send(new CreateImageCommand()
            {
                BrowserId = Guid.NewGuid().ToString(),
                Title = "Yoyoyoyo",
                Content = new Website.Domain.Content.Content()
                {
                    Type = Website.Domain.Content.Content.ContentType.Image,
                    Data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 5, 6, 5, 6, 56 }
                }
            }) as ImageInterface;

            Assert.IsNotNull(imageInterface);

            //test the state was initially processing
            Assert.AreEqual(ImageStatus.Processing, imageInterface.Status);

            //test the state is failed
            Assert.AreEqual(ImageStatus.Failed,
                ResolutionExtensions.Get<GenericQueryServiceInterface>(kernel).FindById<Website.Domain.Content.Image>(imageInterface.Id).Status);

            return imageInterface.Id;
        }

        [Test]
        public void ImageProcessSetsDimensionOnImageDomainEntity()
        {
            using (var bitmap = new Bitmap(600, 600))
            {
                AssertImage(bitmap, AssetImageProcessSetsDimensionOnImageDomainEntity);
            } 
        }

        public void AssetImageProcessSetsDimensionOnImageDomainEntity(Guid guid, Dictionary<string, byte[]> dictionary)
        {
            var query = Kernel.Get<GenericQueryServiceInterface>();
            var img = query.FindById<Website.Domain.Content.Image>(guid.ToString());
            Assert.That(img.AvailableDimensions != null);
            Assert.That(img.AvailableDimensions.Count, 
                Is.EqualTo( 1 + 
                (Enum.GetValues(typeof(ThumbSize)).Length * Enum.GetValues(typeof(ThumbOrientation)).Length))
                );
        }

        //ImageProcessSetMetaDataCommandHandler
        [Test]
        public void ImageProcessSetMetaDataCommandHandlerMetaDataIsSetOnAllImages()
        {
            using(var bitmap = new Bitmap(600,600))
            {
                AssertImage(bitmap, TestAfterMetaUpdateAllImagesHaveMetaData);
            }     
        }

        private void TestAfterMetaUpdateAllImagesHaveMetaData(Guid guid, Dictionary<string, byte[]> dictionary)
        {
            var cmd = new SetImageMetaDataCommand()
                          {
                              Id = guid.ToString(),
                              MessageId = Guid.NewGuid().ToString(),
                              Location = new Website.Domain.Location.Location(22, 22),
                              Title = "Test Title"
                          };

            Kernel.Get<CommandBusInterface>().Send(cmd);

            foreach (var bytese in dictionary)
            {
                using (var ms = new MemoryStream(bytese.Value))
                {
                    using (var conv = Image.FromStream(ms))
                    {
                        var exifImage = new ExifImage(conv);
                        Assert.AreEqual(cmd.Title, exifImage.GetImageTitle());

                        var retloc = new Website.Domain.Location.Location(exifImage.GetLongitude().Value, exifImage.GetLatitude().Value);

                        Assert.AreEqual(cmd.Title, exifImage.GetImageTitle());
                        Assert.AreEqual(cmd.Location, retloc); 
                    }
                }
            }
        }

    }
}
