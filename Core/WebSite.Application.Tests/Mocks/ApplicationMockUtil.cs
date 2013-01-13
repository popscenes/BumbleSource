using System.Collections.Generic;
using System.IO;
using Moq;
using Ninject.MockingKernel.Moq;
using Website.Application.Content;

namespace Website.Application.Tests.Mocks
{
    public static class ApplicationMockUtil
    {
        public static Dictionary<string, byte[]> SetupMockBlobStorage(MoqMockingKernel kernel)
        {
            kernel.Unbind<BlobStorageInterface>();
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
            mockstore.Setup(s => s.SetBlobFromStream(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<BlobProperties>()))
                     .Returns<string, Stream, BlobProperties>((s, stream, props) =>
                         {
                             if (storage.ContainsKey(s))
                                 storage.Remove(s);

                             using (var ms = new MemoryStream())
                             {
                                 stream.CopyTo(ms);
                                 ms.Flush();
                                 storage.Add(s, ms.GetBuffer());                                 
                             }
                             return true;
                         });
            kernel.Bind<BlobStorageInterface>().ToConstant(mockstore.Object);
            return storage;
        }
    }
}
