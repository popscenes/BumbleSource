using System;
using System.Collections.Generic;
using System.Text;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Venue;
using Website.Domain.Payment;
using Website.Infrastructure.Domain;
using Website.Domain.Tag;


namespace PostaFlya.Domain.Flier
{
    [Serializable]
    public class Flier : EntityBase<FlierInterface>, AggregateRootInterface, FlierInterface
    {
        public Flier()
        {
            Id = Guid.NewGuid().ToString();
            Tags = new Tags();
            ExtendedProperties = new Dictionary<string, object>();
            ImageList = new List<FlierImage>();
            CreateDate = DateTime.UtcNow;
            Boards = new List<BoardFlier>();
            Features = new HashSet<EntityFeatureCharge>();
            EventDates = new List<DateTimeOffset>();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public Tags Tags { get; set; }
        public Guid? Image { get; set; }
        public List<DateTimeOffset> EventDates { get; set; }

        public string BrowserId { get; set; }
        public DateTime CreateDate { get; set; }

        //will be used for repost
        //public DateTime EffectiveDate { get; set; }
        
        public FlierStatus Status { get; set; }
        public FlierBehaviour FlierBehaviour { get; set; }
        public List<FlierImage> ImageList { get; set; }
        public string ExternalSource { get; set; }
        public string ExternalId { get; set; }

        public Dictionary<string, object> ExtendedProperties { get; set; }
        public int NumberOfClaims { get; set; }
        public int NumberOfComments { get; set; }
        public List<BoardFlier> Boards { get; set; }
        public HashSet<EntityFeatureCharge> Features { get; set; }
        public bool HasLeadGeneration { get; set; }
        public int LocationRadius { get; set; }
        public bool EnableAnalytics { get; set; }
        public string TinyUrl { get; set; }

        public List<UserLink> UserLinks { get; set; }


    }
}