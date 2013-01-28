using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostaFlya.Domain.Flier.Analytic
{
    public class FlierAnalyticInfo
    {
        int TotalDetailVisits { get; set; }
        List<AnalyticTrackingSummary> Entries { get; set; }
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
            {
                summary.TotalDetailViews++;
                TotalDetailVisits++;
            }
                
            
             
            return summary;
        }
    }
}
