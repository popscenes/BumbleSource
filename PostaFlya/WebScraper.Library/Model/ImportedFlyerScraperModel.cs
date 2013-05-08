using System;
using System.Collections.Generic;
using System.Drawing;

namespace WebScraper.Library.Model
{
    public class ImportedFlyerScraperModel
    {
        public Guid Id { get; set; }   
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<DateTime> EventDates { get; set; }
        public LocationScraperModel Location { get; set; }
        public List<UserLinkScraperModel> Links { get; set; }
        public string Tags { get; set; }
        public string Source { get; set; }
        public Uri SourceDetailPage { get; set; }

        public VenueInfoScraperModel VenueInfo { get; set; }

        public bool IsValid()
        {

            var ret = true;

            try
            {
                ret = ret && !string.IsNullOrWhiteSpace(Title);
                ret = ret && !string.IsNullOrWhiteSpace(Description);
                ret = ret && !string.IsNullOrWhiteSpace(ImageUrl);
                ret = ret && !string.IsNullOrWhiteSpace(Tags);
                ret = ret && !string.IsNullOrWhiteSpace(VenueInfo.PlaceName);
                ret = ret && EventDates.Count > 0;
                ret = ret && Location.IsValid();
                ret = ret && VenueInfo.Address.IsValid();
            }
            catch (Exception)
            {
                return false;
            }
            

            return ret;
        }

    }
}
