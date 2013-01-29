using System;
using Website.Domain.Location;

namespace PostaFlya.Domain.Flier.Analytic
{
    public class AnalyticTrackingSummary
    {
        public string TrackingId { get; set; }
        public string BrowserId { get; set; }
        public int TotalDetailViews { get; set; }
        public Location Location { get; set; }
        public string UserAgent { get; set; }
        public bool LocationFromSearch { get; set; }
        public FlierAnalyticSourceAction InitialSource { get; set; }
        public DateTimeOffset InitialVisitTime { get; set; }
    }
}