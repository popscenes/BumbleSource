using System;
using System.Collections.Generic;
using System.Drawing;

namespace WebScraper.Library.Model
{
    public class ImportedFlyerScraperModel
    {
        public bool Import { get; set; }
        public Image Image { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<DateTime> EventDates { get; set; } 
    }
}
