namespace WebScraper.Library.Model
{
    public class VenueInfoScraperModel
    {
        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public string FirstName { get; set; }

        public string MiddleNames { get; set; }

        public string Surname { get; set; }

        public LocationScraperModel Address { get; set; }

        public string WebSite { get; set; }

        public string Source { get; set; }
        public string SourceId { get; set; }
        public string SourceUrl { get; set; }
        public string SourceImageUrl { get; set; }
        public string BoardFriendlyId { get; set; }

        public string PlaceName { get; set; }
    }
}
