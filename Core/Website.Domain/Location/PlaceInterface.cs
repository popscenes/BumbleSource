namespace Website.Domain.Location
{
    public static class PlaceInterfaceExtensions
    {
        public static void CopyFieldsFrom(this PlaceInterface target, PlaceInterface source)
        {
            target.PlaceName = source.PlaceName;
        }

    }
    public interface PlaceInterface
    {
        string PlaceName { get; set; }
    }
}