using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Website.Common.Model;

namespace PostaFlya.Areas.WebApi.Flyers.Model
{
    [DataContract]
    [Serializable]
    public class FlyersByDateContent
    {      
        [DataMember]
        public List<FlyersByDate> Dates { get; set; }

        [DataContract]
        [Serializable]
        public class FlyersByDate
        {
            [DataMember]
            public DateTimeOffset Date { get; set; }
            [DataMember]
            public List<string> FlyerIds { get; set; } 
        }

        [DataMember]
        public Dictionary<string, FlyerSummaryModel> Flyers { get; set; }
    }
}