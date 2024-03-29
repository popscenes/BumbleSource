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

        public static GeoCoords AsGeoCoords(this LocationInterface source)
        {
            return new GeoCoords(){Latitude = source.Latitude, Longitude = source.Longitude};
        }
    }

    public interface LocationInterface
    {
        double Longitude { get; set; }
        double Latitude { get; set; }
    }

    public class GeoCoords : LocationInterface
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}