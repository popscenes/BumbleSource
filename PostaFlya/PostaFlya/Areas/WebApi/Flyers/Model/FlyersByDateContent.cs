using System;
using System.Collections.Generic;

namespace PostaFlya.Areas.WebApi.Flyers.Model
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