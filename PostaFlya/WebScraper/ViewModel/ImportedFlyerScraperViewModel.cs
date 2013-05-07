using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using WebScraper.Library.Model;

namespace WebScraper.ViewModel
{
    public class ImportedFlyerScraperViewModel
    {
        public bool Import { get; set; }
        public BitmapImage Image { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<DateTime> EventDates { get; set; }
        public LocationScraperModel Location { get; set; }
        public VenueInfoScraperModel VenueInfo { get; set; }
    }
}
