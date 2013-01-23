using System;
using PostaFlya.Domain.Flier.Analytic;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Command;

namespace PostaFlya.Domain.Flier.Command
{
    [Serializable]
    public class FlierAnalyticCommand : DefaultCommandBase
    {
        public FlierAnalyticCommand()
        {
            Time = DateTimeOffset.Now;
        }
        public string FlierId { get; set; }
        public string TrackingId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public FlierAnalyticSourceAction SourceAction { get; set; }
        public DateTimeOffset Time { get; set; }
        public Location Location { get; set; }
        public Browser Browser { get; set; }
    }
}