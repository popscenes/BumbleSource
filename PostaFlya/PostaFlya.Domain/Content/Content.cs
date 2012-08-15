namespace PostaFlya.Domain.Content
{
    public class Content
    {
        public enum ContentType
        {
            Image,
            ImageFromUrl
        }

        public ContentType Type { get; set; }
        public byte[] Data { get; set; }
    }
}