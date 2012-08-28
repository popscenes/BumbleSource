using System;
using Website.Infrastructure.Domain;

namespace Website.Domain.Claims
{
    [Serializable]
    public class Claim : EntityBase<ClaimInterface>, ClaimInterface 
    {
        public string EntityTypeTag{ get; set; }
        public string AggregateId { get; set; }
        public string BrowserId { get; set; }
        public string ClaimContext { get; set; }
        public DateTime ClaimTime { get; set; }
    }
}
