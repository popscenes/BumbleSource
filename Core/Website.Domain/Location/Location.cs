using System;

namespace Website.Domain.Location
{


    [Serializable]
    public class Location : LocationAndAddressInterface
    {
        
        private const double Invalid = -200;

        public Location()
        {
            Longitude = Invalid;
            Latitude = Invalid;
            Description = "";
        }

        public Location(LocationAndAddressInterface source)
        {
            this.CopyFieldsFrom(source);
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
            if(arr.Length > 2)
                Description = arr[2];
            if(arr.Length > 3)
                this.SetAddressFromStringFormat(arr[3]);

        }

        public override string ToString()
        {
            var ret = Latitude + ":" + Longitude + ":" + Description;
            if (this.HasAddressParts())
                ret += ":" + this.GetAddressStringFormat();
            return ret;

        }

        public double Longitude { get; set; }
        
        public double Latitude { get; set; }

        private string _description;
        public string Description
        {
            get {
                if (string.IsNullOrEmpty(_description) && this.HasAddressParts())
                    _description = this.GetAddressDescription();
                return _description;
            } 
            set { _description = value; }
        }

        public string StreetAddress { get; set; }
        public string Locality { get; set; }
        public string Region { get; set; }
        public string PostCode { get; set; }
        public string CountryName { get; set; }


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
            return Longitude.GetHashCode() ^ Latitude.GetHashCode();
        }

        public bool IsValid {
            get
            {
                return this.IsValid();
            }
        }
    }
}