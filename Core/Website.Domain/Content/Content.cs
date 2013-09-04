using System.IO;

namespace Website.Domain.Content
{
    public class Content
    {
        public enum ContentType
        {
            Image,
            ImageFromUrl
        }

        public ContentType Type { get; set; }
        public string OriginalFileName { get; set; }
        public string Extension {
            get { return OriginalFileName == null ? "" : Path.GetExtension(OriginalFileName).Substring(1); }
        }
        public byte[] Data { get; set; }
    }
}