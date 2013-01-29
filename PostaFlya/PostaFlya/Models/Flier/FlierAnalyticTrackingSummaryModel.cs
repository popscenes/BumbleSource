using System;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Models.Location;
using Resources = PostaFlya.Properties.Resources;

namespace PostaFlya.Models.Flier
{
    public static class FlierAnalyticTrackingSummaryExtensions
    {
        public static FlierAnalyticTrackingSummaryModel ToModel(this AnalyticTrackingSummary info)
        {
            return new FlierAnalyticTrackingSummaryModel()
            {
                TrackingId = info.TrackingId,
                TotalDetailViews = info.TotalDetailViews,
                Location = info.Location.ToViewModel(),
                LocationSource = GetLocationSourceDesc(info),
                InitialSource = GetSourceDescription(info.InitialSource),
                InitialVisitTime = info.InitialVisitTime,
                UserAgent = info.UserAgent
            };
        }

        public static string GetLocationSourceDesc(AnalyticTrackingSummary info)
        {
            if (!info.Location.IsValid)
                return Properties.Resources.AnalyticTrackingSummaryExtensions_GetLocationSourceDesc_LocationNotKnown;
            return info.LocationFromSearch
                ? Properties.Resources.AnalyticTrackingSummaryExtensions_GetLocationSourceDesc_LocationFromSearch
                : Properties.Resources.AnalyticTrackingSummaryExtensions_GetLocationSourceDesc_LocationFromDevice;
        }

        public static string GetSourceDescription(FlierAnalyticSourceAction sourceAction)
        {
            switch (sourceAction)
            {
                case FlierAnalyticSourceAction.Unknown:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_Unknown;
                case FlierAnalyticSourceAction.QrCodeSrcCodeOnly:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_QrCodeSrcCodeOnly;
                case FlierAnalyticSourceAction.QrCodeSrcOnFlierWithTearOffs:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_QrCodeSrcOnFlierWithTearOffs;
                case FlierAnalyticSourceAction.QrCodeSrcTearOff:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_QrCodeSrcTearOff;
                case FlierAnalyticSourceAction.QrCodeSrcOnFlierWithoutTearOffs:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_QrCodeSrcOnFlierWithoutTearOffs;
                case FlierAnalyticSourceAction.TinyUrl:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_TinyUrl;
                case FlierAnalyticSourceAction.LocationTrack:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_LocationTrack;
                case FlierAnalyticSourceAction.TinyUrlByApi:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_TinyUrlByApi;
                case FlierAnalyticSourceAction.IdByApi:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_IdByApi;
                case FlierAnalyticSourceAction.IdByBulletin:
                    return Properties.Resources.AnalyticTrackingSummaryExtensions_GetSourceDescription_IdByBulletin;
                default:
                    throw new ArgumentOutOfRangeException("sourceAction");
            }
        }
    }

    public class FlierAnalyticTrackingSummaryModel
    {
        public string TrackingId { get; set; }
        [Display(Name = "FlierAnalyticTrackingSummaryModel_TotalDetailViews", ResourceType = typeof (Properties.Resources))]
        public int TotalDetailViews { get; set; }
        public LocationModel Location { get; set; }
        public string UserAgent { get; set; }
        public string LocationSource { get; set; }
        public string InitialSource { get; set; }
        public DateTimeOffset InitialVisitTime { get; set; }
        public static FlierAnalyticTrackingSummaryModel DefaultForTemplate()
        {
            return new FlierAnalyticTrackingSummaryModel()
                {
                    Location = new LocationModel()
                };
        }
    }
}