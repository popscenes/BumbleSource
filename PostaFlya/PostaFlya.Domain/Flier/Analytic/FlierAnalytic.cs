using System;
using System.Collections.Generic;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier.Analytic
{
    public static class FlierAnalyticInterfaceExtensions
    {
        public static void CopyFieldsFrom(this FlierAnalyticInterface target, FlierAnalyticInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            AggregateInterfaceExtensions.CopyFieldsFrom(target, source);
            BrowserIdInterfaceExtensions.CopyFieldsFrom(target, source);
            target.TemporaryBrowser = source.TemporaryBrowser;
            target.TrackingId = source.TrackingId;
            target.IpAddress = source.IpAddress;
            target.UserAgent = source.UserAgent;
            target.SourceAction = source.SourceAction;
            target.Time = source.Time;
            target.Location = source.Location;
            target.LocationFromSearch = source.LocationFromSearch;
        }

        public static FlierAnalyticInfo ToInfo(this IList<FlierAnalytic> analytics)
        {
            return new FlierAnalyticInfo(analytics);
        }

        public static bool IsSourceADetailView(this FlierAnalyticInterface source)
        {
            return source.SourceAction == FlierAnalyticSourceAction.IdByApi
                   || source.SourceAction == FlierAnalyticSourceAction.TinyUrlByApi
                   || source.SourceAction == FlierAnalyticSourceAction.IdByBulletin;
        }

    }

    public interface FlierAnalyticInterface : EntityInterface, AggregateInterface, BrowserIdInterface
    {
        bool TemporaryBrowser { get; set; }
        string TrackingId { get; set; }
        string IpAddress { get; set; }
        string UserAgent { get; set; }
        FlierAnalyticSourceAction SourceAction { get; set; }
        DateTimeOffset Time { get; set; }
        Location Location { get; set; }
        bool LocationFromSearch { get; set; }
    }

    [Serializable]
    public class FlierAnalytic : EntityBase<FlierAnalyticInterface>, FlierAnalyticInterface
    {
        public string AggregateId { get; set; }
        public string AggregateTypeTag { get; set; }
        public string BrowserId { get; set; }
        public bool TemporaryBrowser { get; set; }
        public string TrackingId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public FlierAnalyticSourceAction SourceAction { get; set; }
        public DateTimeOffset Time { get; set; }
        public Location Location { get; set; }
        public bool LocationFromSearch { get; set; }
    }

    


}
