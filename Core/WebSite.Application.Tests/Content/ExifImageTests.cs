using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;
using Website.Application.Content;

namespace Website.Application.Tests.Content
{
    [TestFixture]
    public class ExifImageTests
    {
        [Test]
        public void ExifImageExtractLocation()
        {
            var resourceimg = new ExifImage(Properties.Resources.ImageWithLocation);
            var latitude = resourceimg.GetLatitude();
            var longitude = resourceimg.GetLongitude();
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
        }

        [Test]
        public void ExifImageStoreLocation()
        {
            var resourceimg = new ExifImage(Properties.Resources.ImageWithLocation);
            var latitude = resourceimg.GetLatitude();
            var longitude = resourceimg.GetLongitude();
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);

            using (var img = new Bitmap(10, 10))
            {
                var modimg = new ExifImage(img);

                modimg.SetLatitude(latitude.Value);
                modimg.SetLongitude(longitude.Value);

                using(var outstream = new MemoryStream())
                {
                    img.Save(outstream, ImageFormat.Jpeg);

                    using (var loadedImg = Image.FromStream(outstream))
                    {
                        var loadlatitude = modimg.GetLatitude();
                        var loadlongitude = modimg.GetLongitude();
                        Assert.IsNotNull(loadlatitude);
                        Assert.IsNotNull(loadlongitude);
                        Assert.AreEqual(latitude.Value, loadlatitude.Value);
                        Assert.AreEqual(longitude.Value, loadlongitude.Value);
                    }
                }
                
            }
        }

        [Test]
        public void ExifImageStoreTitle()
        {
            using (var img = new Bitmap(10, 10))
            {
                var modimg = new ExifImage(img);

                modimg.SetImageTitle("Test Title");

                using (var outstream = new MemoryStream())
                {
                    img.Save(outstream, ImageFormat.Jpeg);

                    using (var loadedImg = Image.FromStream(outstream))
                    {
                        var loadTitle = modimg.GetImageTitle();
                        Assert.IsNotNull(loadTitle);
                        Assert.AreEqual("Test Title", loadTitle);
                    }
                }

            }
        }

        [Test]
        public void ExifImageStoreDescription()
        {
            using (var img = new Bitmap(10, 10))
            {
                var modimg = new ExifImage(img);

                modimg.SetImageDescription("Test Desc");

                using (var outstream = new MemoryStream())
                {
                    img.Save(outstream, ImageFormat.Jpeg);

                    using (var loadedImg = Image.FromStream(outstream))
                    {
                        var imageDescription = modimg.GetImageDescription();
                        Assert.IsNotNull(imageDescription);
                        Assert.AreEqual("Test Desc", imageDescription);
                    }
                }

            }
        }
    }
}
