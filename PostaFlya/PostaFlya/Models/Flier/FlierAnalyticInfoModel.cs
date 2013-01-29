using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Models.Location;
using Resources = PostaFlya.Properties.Resources;

namespace PostaFlya.Models.Flier
{
    public static class FlierAnalyticInfoExtensions
    {
        public static FlierAnalyticInfoModel ToModel(this FlierAnalyticInfo info)
        {
            return new FlierAnalyticInfoModel()
                {
                    Entries = info.Entries.Select(e => e.ToModel()).ToList()
                };
        }
    }
    public class FlierAnalyticInfoModel
    {
        public FlierAnalyticInfoModel()
        {
            Entries = new List<FlierAnalyticTrackingSummaryModel>();
        }

        public int TotalDetailView
        {
            get{return Entries.Aggregate(0, (i, model) => i + model.TotalDetailViews);}
        }

        public int TotalUniqueDetailView
        {
            get { return Entries.Count; }
        }

        public List<FlierAnalyticTrackingSummaryModel> Entries { get; set; }
        public static FlierAnalyticInfoModel DefaultForTemplate()
        {
            return new FlierAnalyticInfoModel()
                {
                    Entries = new List<FlierAnalyticTrackingSummaryModel>() { FlierAnalyticTrackingSummaryModel.DefaultForTemplate()}
                };
        }
    }


}