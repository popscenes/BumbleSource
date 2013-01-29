using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostaFlya.Domain.Flier.Analytic
{
    public class FlierAnalyticInfo
    {
        public List<AnalyticTrackingSummary> Entries { get; set; }
        public FlierAnalyticInfo(IEnumerable<FlierAnalytic> analytics)
        {
            Entries = new List<AnalyticTrackingSummary>();
            foreach (var trackingGroup in analytics.GroupBy(a => a.TrackingId))
            {
                Entries.Add(
                    trackingGroup
                        .OrderBy(a => a.Time)
                        .Aggregate(new AnalyticTrackingSummary()
                        , AggregateFunc)
                    );
            }          
        }

        private AnalyticTrackingSummary AggregateFunc(AnalyticTrackingSummary summary, FlierAnalytic analytic)
        {
            summary.TrackingId = analytic.TrackingId;
            if (summary.Location == null || !summary.Location.IsValid)
            {
                summary.Location = analytic.Location;
                summary.LocationFromSearch = analytic.LocationFromSearch;
            }

            if (analytic.IsSourceADetailView())
                summary.TotalDetailViews++;

            if (summary.InitialSource == FlierAnalyticSourceAction.Unknown &&
                analytic.SourceAction != FlierAnalyticSourceAction.Unknown)
                summary.InitialSource = analytic.SourceAction;

            if (string.IsNullOrWhiteSpace(summary.UserAgent) && !string.IsNullOrWhiteSpace(analytic.UserAgent))
                summary.UserAgent = analytic.UserAgent;

            if (!analytic.TemporaryBrowser && string.IsNullOrWhiteSpace(summary.BrowserId))
                summary.BrowserId = analytic.BrowserId;

            if (summary.InitialVisitTime == default(DateTimeOffset) && analytic.Time != default(DateTimeOffset))
                summary.InitialVisitTime = analytic.Time;

            return summary;
        }
    }
}
