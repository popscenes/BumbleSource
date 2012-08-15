using System;
using System.Collections.Specialized;
using System.IO;

namespace WebSite.Application.Content
{
    public class BlobProperties
    {
        public string ContentTyp { get; set; }
        public NameValueCollection MetaData { get; set; }
    }

    public interface BlobStorageInterface
    {
        byte[] GetBlob(string id);
        BlobProperties GetBlobProperties(string id);
        bool SetBlob(string id, byte[] bytes, BlobProperties properties = null);
        bool SetBlobProperties(string id, BlobProperties properties);
        bool DeleteBlob(string id);
        int BlobCount { get; }
        bool GetToStream(string id, Stream s);
        Uri GetBlobUri(string id);
        bool Exists(string id);
    }
}