namespace Website.Domain.Location
{
    public static class LocationInterfaceExtensions
    {
        public static bool IsValid(this LocationInterface loc)
        {
            if (loc == null)
                return false;

            return !(loc.Longitude < -180
                         || loc.Longitude > 180
                         || loc.Latitude < -90
                         || loc.Latitude > 90);
        }

        public static void CopyFieldsFrom(this LocationInterface target, LocationInterface source)
        {
            target.Latitude = source.Latitude;
            target.Longitude = source.Longitude;
        }
    }

    public interface LocationInterface
    {
        double Longitude { get; set; }
        double Latitude { get; set; }
    }
}