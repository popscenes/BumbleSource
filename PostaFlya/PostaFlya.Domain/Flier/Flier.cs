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
        private List<DateTimeOffset> _eventDates;

        public Flier()
        {
            Id = Guid.NewGuid().ToString();
            Tags = new Tags();
            ExtendedProperties = new Dictionary<string, object>();
            ImageList = new List<FlierImage>();
            CreateDate = DateTime.UtcNow;
            Boards = new List<BoardFlier>();
            Features = new HashSet<EntityFeatureCharge>();
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Tags: ");
            stringBuilder.Append(Tags);
            stringBuilder.Append(" Location: ");
            stringBuilder.Append(Venue.PlaceName);
            return stringBuilder.ToString();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public Tags Tags { get; set; }
        public Guid? Image { get; set; }
        public List<DateTimeOffset> EventDates
        {
            get
            {
                if (_eventDates == null)
                    _eventDates = new List<DateTimeOffset>();

                if (_eventDates.Count == 0 && EffectiveDate != default(DateTime) && EffectiveDate != CreateDate)
                    _eventDates.Add(EffectiveDate);
                return _eventDates;
            }
            set { _eventDates = value; }
        }

        public string BrowserId { get; set; }
        public DateTime CreateDate { get; set; }

        //will be used for repost
        public DateTime EffectiveDate { get; set; }
        
        public FlierStatus Status { get; set; }
        public FlierBehaviour FlierBehaviour { get; set; }
        public List<FlierImage> ImageList { get; set; }
        public string ExternalSource { get; set; }
        public string ExternalId { get; set; }

        public Dictionary<string, object> ExtendedProperties { get; set; }
        public int NumberOfClaims { get; set; }
        public int NumberOfComments { get; set; }
        public VenueInformation Venue { get; set; }
        public List<BoardFlier> Boards { get; set; }
        public HashSet<EntityFeatureCharge> Features { get; set; }
        public bool HasLeadGeneration { get; set; }
        public int LocationRadius { get; set; }
        public bool EnableAnalytics { get; set; }
        public string TinyUrl { get; set; }

        public List<UserLink> UserLinks { get; set; }


    }
}