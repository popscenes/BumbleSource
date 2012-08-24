using System;

namespace Website.Domain.Location
{
    public static class LocationInterfaceExtension
    {
        public static bool IsValid(this LocationInterface loc)
        {
            return !(loc.Longitude < -180
                         || loc.Longitude > 180
                         || loc.Latitude < -90
                         || loc.Latitude > 90);
        }
    }

    public interface LocationInterface
    {
        double Longitude { get; set; }
        double Latitude { get; set; }
    }

    [Serializable]
    public class Location : LocationInterface
    {
        
        private const double Invalid = -200;

        public Location()
        {
            Longitude = Invalid;
            Latitude = Invalid;
            Description = "";
        }

        public Location(Location source)
        {
            Longitude = source.Longitude;
            Latitude = source.Latitude;
            Description = source.Description;
        }

        public Location(double longitude, double latitude, string description = "")
            : this()
        {
            Longitude = longitude;
            Latitude = latitude;
            Description = description;
        }

        public Location(string coords)
            : this()
        {
            var arr = coords.Split(':');
            if (arr.Length < 2)
                return;

            Latitude = Double.Parse(arr[0]);
            Longitude = Double.Parse(arr[1]);
            for (int i = 2; i < arr.Length; i++)
                Description = Description + ':' + arr[i];
        }

        public override string ToString()
        {
            return Latitude + ":" + Longitude + ":" + Description;
        }

        public double Longitude { get; set; }
        
        public double Latitude { get; set; }
        
        public string Description { get; set; }          

        // override object.Equals
        public override bool Equals(object obj)
        {
            var equals = false;
            var location = obj as Location;
            if (location == null)
                return false;

            if (Math.Abs(this.Latitude - location.Latitude) < 0.00001 && Math.Abs(this.Longitude - location.Longitude) < 0.00001)
            {
                equals = true;
            }

            return equals;

        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public bool IsValid {
            get
            {
                return this.IsValid();
            }
        }
    }
}