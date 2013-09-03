using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Website.Application.Content
{
    public class BlobProperties
    {
        public string ContentTyp { get; set; }
        public IDictionary<string, string> MetaData { get; set; }

        public static BlobProperties JpegContentTypeDefault
        {
            get
            {
                return new BlobProperties() { ContentTyp = "image/jpeg" };
            }
        }

        public static BlobProperties ImageContentTypeFortExtension(String extension)
        {
            return new BlobProperties() { ContentTyp = "image/" + extension };
        }
    }

    public interface BlobStorageInterface
    {
        byte[] GetBlob(string id);
        BlobProperties GetBlobProperties(string id);
        bool SetBlob(string id, byte[] bytes, BlobProperties properties = null);
        bool SetBlobFromStream(string id, Stream stream, BlobProperties properties = null);
        bool SetBlobProperties(string id, BlobProperties properties);
        bool DeleteBlob(string id);
        int BlobCount { get; }
        bool GetToStream(string id, Stream s);
        Uri GetBlobUri(string id, bool cdnUri = true);
        bool Exists(string id);
    }
}