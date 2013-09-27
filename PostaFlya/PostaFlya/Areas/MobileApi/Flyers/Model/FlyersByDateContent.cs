using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PostaFlya.Domain.Flier;
using Website.Common.Model;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{
    [Serializable]
    [DataContract]
    public class FlyersByDateContent
    {      
        [DataMember]
        public List<FlyersByDate> Dates { get; set; }

        [Serializable]
        [DataContract]
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