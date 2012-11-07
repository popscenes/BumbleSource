namespace Website.Domain.Browser
{
    public interface BrowserIdInterface
    {
        string BrowserId { get; set; }
    }

    public static class BrowserIdInterfaceExtensions
    {
        public static void CopyFieldsFrom(this BrowserIdInterface target, BrowserIdInterface source)
        {
            target.BrowserId = source.BrowserId;
        }
    }
}
