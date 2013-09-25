using System;

namespace Website.Domain.Location
{
    public class GeoPoints
    {
        public GeoPoints()
        {
        }

        public GeoPoints(LocationInterface a, LocationInterface b, Func<LocationInterface, LocationInterface, double> distance)
        {
            PointA = a.AsGeoCoords();
            PointB = b.AsGeoCoords();
            this.Distance = distance(PointA, PointB); 
                
        }

        public double Distance { get; set; }
        public GeoCoords PointA { get; set; }
        public GeoCoords PointB { get; set; }
    }
}