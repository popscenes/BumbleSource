using System;

namespace Website.Domain.Location
{
    public class LocationService : LocationServiceInterface
    {
        public bool IsWithinDistance(Location location, int distance)
        {
            var def = GetBoundingBox(location, distance);
            return IsWithinBoundingBox(def, location);
        }

        public bool IsWithinDefaultDistance(Location location)
        {
            var def = GetDefaultBox(location);
            return IsWithinBoundingBox(def, location);
        }

        public bool IsWithinBoundingBox(BoundingBox boundingBox, Location location)
        {
            return location.Latitude >= boundingBox.Min.Latitude
               && location.Latitude <= boundingBox.Max.Latitude
               && location.Longitude >= boundingBox.Min.Longitude
               && location.Longitude <= boundingBox.Max.Longitude;
        }

        public BoundingBox GetDefaultBox(Location location)
        {
            return GetBoundingBoxDefault(location.Latitude, location.Longitude);
        }

        public BoundingBox GetBoundingBox(Location location, double distance)
        {
            return GetBoundingBox(location.Latitude, location.Longitude, distance);
        }

        private const int DefHalfDist = 15;
        //major semi-axis
        private const double WGS84_a = 6378137.0;
        //minor semi-axis
        private const double WGS84_b = 6356752.3;


        private static double deg2rad(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

        private static double rad2deg(double radians)
        {
            return 180.0 * radians / Math.PI;
        }

        //approx earths radius as a given latitude
        private static double WGS84EarthRadius(double lat)
        {
            double An = WGS84_a * WGS84_a * Math.Cos(lat);
            double Bn = WGS84_b * WGS84_b * Math.Sin(lat);
            double Ad = WGS84_a * Math.Cos(lat);
            double Bd = WGS84_b * Math.Sin(lat);

            return Math.Sqrt((An * An + Bn * Bn) / (Ad * Ad + Bd * Bd));
        }

        private static BoundingBox GetBoundingBoxDefault(double DegLat, double DegLong)
        {
            return GetBoundingBox(DegLat, DegLong, DefHalfDist);
        }

        private static BoundingBox GetBoundingBox(double DegLat, double DegLong, double HalfSideKms)
        {
            double lat = deg2rad(DegLat);
            double lon = deg2rad(DegLong);
            double halfSide = 1000 * HalfSideKms;
            //Radius of Earth at given latitude    
            double radius = WGS84EarthRadius(lat);
            //Radius of the parallel at given latitude    
            double pradius = radius * Math.Cos(lat);
            double latMin = lat - halfSide / radius;
            double latMax = lat + halfSide / radius;
            double lonMin = lon - halfSide / pradius;
            double lonMax = lon + halfSide / pradius;

            return new BoundingBox()
            {
                Min = new Location(rad2deg(lonMin), rad2deg(latMin)),
                Max = new Location(rad2deg(lonMax), rad2deg(latMax))
            };
        }
    }
}