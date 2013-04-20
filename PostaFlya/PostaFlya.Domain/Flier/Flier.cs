using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Venue;
using Website.Domain.Claims;
using Website.Domain.Contact;
using Website.Domain.Payment;
using Website.Infrastructure.Domain;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;


namespace PostaFlya.Domain.Flier
{
    [Serializable]
    public class Flier : EntityBase<FlierInterface>, FlierInterface
    {
        private List<DateTime> _eventDates = new List<DateTime>();

        public Flier()
        {
            Id = Guid.NewGuid().ToString();
            Tags = new Tags();
            ExtendedProperties = new Dictionary<string, object>();
            ImageList = new List<FlierImage>();
            CreateDate = DateTime.UtcNow;
            Boards = new HashSet<string>();
            Features = new HashSet<EntityFeatureCharge>();
        }

        public Flier(Location location)
            : this()
        {
            Location = location;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Tags: ");
            stringBuilder.Append(Tags);
            stringBuilder.Append(" Location: ");
            stringBuilder.Append(Location);
            return stringBuilder.ToString();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public Tags Tags { get; set; }
        public Location Location { get; set; }
        public Guid? Image { get; set; }
        public List<DateTime> EventDates
        {
            get
            {
                if(_eventDates.Count == 0 && EffectiveDate != default(DateTime))
                    _eventDates.Add(EffectiveDate);
                return _eventDates;
            }
            set { _eventDates = value; }
        }

        public string BrowserId { get; set; }
        public DateTime CreateDate { get; set; }

        [Obsolete("Use EventDates, will be removed", false)]
        public DateTime EffectiveDate { get; set; }
        
        public FlierStatus Status { get; set; }
        public FlierBehaviour FlierBehaviour { get; set; }
        public List<FlierImage> ImageList { get; set; }
        public string ExternalSource { get; set; }
        public string ExternalId { get; set; }

        public Dictionary<string, object> ExtendedProperties { get; set; }
        public int NumberOfClaims { get; set; }
        public int NumberOfComments { get; set; }
        public VenueInformation ContactDetails { get; set; }
        public HashSet<string> Boards { get; set; }
        public HashSet<EntityFeatureCharge> Features { get; set; }
        public bool HasLeadGeneration { get; set; }
        public int LocationRadius { get; set; }
        public bool EnableAnalytics { get; set; }
        public string TinyUrl { get; set; }




    }
}