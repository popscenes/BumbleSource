namespace Website.Domain.TinyUrl
{
    public static class TinyUrlInterfaceExtensions
    {
        public static void CopyFieldsFrom(this TinyUrlInterface target, TinyUrlInterface source)
        {
            target.TinyUrl = source.TinyUrl;
        }
    }
    public interface TinyUrlInterface
    {
        string TinyUrl { get; set; }
    }
}