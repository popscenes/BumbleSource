using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using PostaFlya.Application.Domain.Content;

namespace PostaFlya.Application.Domain.Tests.Content
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

            Assert.AreEqual(content.Type, PostaFlya.Domain.Content.Content.ContentType.Image);
            Assert.AreEqual(content.Data.Length, 138851);

        }

        
    }
}
