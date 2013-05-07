namespace WebScraper.Library.Model
{
    public class LocationScraperModel
    {

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public string StreetNumber { get; set; }
        public string Street { get; set; }
        public string Locality { get; set; }
        public string Region { get; set; }
        public string PostCode { get; set; }
        public string CountryName { get; set; }

        public  bool IsValid()
        {
            return !(this.Longitude < -180
                         || this.Longitude > 180
                         || this.Latitude < -90
                         || this.Latitude > 90);
        }

    }
}
