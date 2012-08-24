using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Domain.Content;

namespace Website.Application.Domain.Tests.Content
{

    [TestFixture]
    public class ImageUrlContentRetrieverTest
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Test]
        public void ImageUrlContentRetrieverGetContentTest()
        {
            //lenna test pic http://en.wikipedia.org/wiki/Lenna

            var contentRetriever = new ImageUrlContentRetriever();
            var content = contentRetriever.GetContent("http://www.lenna.org/full/len_full.jpg");

            Assert.AreEqual(content.Type, Website.Domain.Content.Content.ContentType.Image);
            Assert.AreEqual(content.Data.Length, 138851);

        }

        
    }
}
