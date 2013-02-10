using System;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Models.Location;

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
            if (info.Location == null || !info.Location.IsValid)
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
        [Display(Name = "FlierAnalyticTrackingSummaryModel_TrackingId_TrackingId", ResourceType = typeof(Properties.Resources))]
        public string TrackingId { get; set; }
        
        [Display(Name = "FlierAnalyticTrackingSummaryModel_TotalDetailViews", ResourceType = typeof (Properties.Resources))]
        public int TotalDetailViews { get; set; }

        [Display(Name = "FlierAnalyticTrackingSummaryModel_Location", ResourceType = typeof(Properties.Resources))]
        public LocationModel Location { get; set; }

        [Display(Name = "FlierAnalyticTrackingSummaryModel_LocationSource", ResourceType = typeof(Properties.Resources))]
        public string LocationSource { get; set; }

        [Display(Name = "FlierAnalyticTrackingSummaryModel_UserAgent", ResourceType = typeof(Properties.Resources))]
        public string UserAgent { get; set; }

        [Display(Name = "FlierAnalyticTrackingSummaryModel_InitialSource", ResourceType = typeof(Properties.Resources))]
        public string InitialSource { get; set; }

        [Display(Name = "FlierAnalyticTrackingSummaryModel_InitialVisitTime", ResourceType = typeof(Properties.Resources))]        
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