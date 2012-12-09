using System;
using System.Collections.Generic;
using Website.Domain.Payment;
using Website.Infrastructure.Domain;

namespace Website.Domain.Claims
{
    [Serializable]
    public class Claim : EntityBase<ClaimInterface>, ClaimInterface 
    {
        public string AggregateTypeTag{ get; set; }
        public string AggregateId { get; set; }
        public string BrowserId { get; set; }
        public string ClaimContext { get; set; }
        public DateTime ClaimTime { get; set; }
        public String ClaimMessage { get; set; }
        public HashSet<EntityFeatureCharge> Features { get; set; }
    }
}
