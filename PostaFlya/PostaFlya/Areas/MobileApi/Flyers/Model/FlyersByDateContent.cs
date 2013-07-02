using System;
using System.Collections.Generic;
using PostaFlya.Domain.Flier;
using Website.Common.Model;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{

    public class FlyersByDateContent
    {      
        public List<FlyersByDate> Dates { get; set; }
        public class FlyersByDate
        {
            public DateTimeOffset Date { get; set; }
            public List<string> FlyerIds { get; set; } 
        }

        public Dictionary<string, FlyerSummaryModel> Flyers { get; set; }
    }
}