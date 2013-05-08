namespace Website.Domain.Location
{
    public static class PlaceInterfaceExtensions
    {
        public static void CopyFieldsFrom(this PlaceInterface target, PlaceInterface source)
        {
            target.PlaceName = source.PlaceName;
            target.UtcOffset = source.UtcOffset;
        }

    }
    public interface PlaceInterface
    {
        int UtcOffset { get; set; }
        string PlaceName { get; set; }
    }
}