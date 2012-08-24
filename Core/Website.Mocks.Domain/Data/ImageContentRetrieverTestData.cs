using System.Drawing.Imaging;
using System.IO;
using Ninject.MockingKernel.Moq;
using Website.Application.Domain.Content;
using Website.Domain.Content;

namespace Website.Mocks.Domain.Data
{
    public static class ImageContentRetrieverTestData
    {
        public static void SetupContentRetrieverForImage(MoqMockingKernel kernel, System.Drawing.Bitmap srcbitmap)
        {
            kernel.Unbind<RequestContentRetrieverFactoryInterface>();
            kernel.Unbind<RequestContentRetrieverInterface>();

            var bitmap = srcbitmap;

            var contentRetrieverFactory = kernel.GetMock<RequestContentRetrieverFactoryInterface>();
            var contentRetriever = kernel.GetMock<RequestContentRetrieverInterface>();
            contentRetriever.Setup(cr => cr.GetContent()).Returns(
                () =>
                {
                    byte[] data = null;
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Bmp);
                        data = ms.ToArray();
                    }
                    return new Content()
                    {
                        Data = data,
                        Type =
                            Content.ContentType
                            .Image
                    };

                });

            contentRetrieverFactory.Setup(fi => fi.GetRetriever(Content.ContentType.Image))
                .Returns(() => contentRetriever.Object);

            kernel.Bind<RequestContentRetrieverFactoryInterface>().ToConstant(contentRetrieverFactory.Object);
            kernel.Bind<RequestContentRetrieverInterface>().ToConstant(contentRetriever.Object); ;
        }
    }
}
