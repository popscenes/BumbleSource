using System.Collections.Generic;

namespace PostaFlya.Areas.WebApi.Flyers.Model
{
    public class FlyersByFeaturedContent
    {
        public Dictionary<string, FlyerSummaryModel> Flyers { get; set; }
        public List<FlyersByFeature> FeatureGroups { get; set; }
        public class FlyersByFeature
        {
            public string FeatureGroup { get; set; }
            public List<string> FlyerIds { get; set; }
        }

    }
}