namespace TravelSite.Applications.Intergrations.Expedia
{
    public class ExpediaProperty
    {
        public int EANHotelId { get; set; }
        public int SequenceNumber { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string AirportCode { get; set; }
        public string PropertyCategory { get; set; }
        public string PropertyCurrency { get; set; }
        public double StarRating { get; set; }
        public int Confidence { get; set; }
        public string SupplierType { get; set; }
        public string Location { get; set; }
        public string ChainCodeId { get; set; }
        public int RegionId { get; set; }
        public double HighRate { get; set; }
        public double LowRate { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
    }
}