using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Behaviour;
using Website.Domain.Contact;
using Website.Infrastructure.Domain;
using Website.Domain.Location;
using Website.Domain.Tag;


namespace PostaFlya.Domain.Flier
{
    [Serializable]
    public class Flier : EntityBase<FlierInterface>, FlierInterface
    {

        public Flier()
        {
            Id = Guid.NewGuid().ToString();
            Tags = new Tags();
            ExtendedProperties = new Dictionary<string, object>();
            ImageList = new List<FlierImage>();
            CreateDate = DateTime.UtcNow;
            PaymentOptions = new HashSet<PaymentOption>();
            Boards = new HashSet<string>();
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
        public string BrowserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public FlierStatus Status { get; set; }
        public FlierBehaviour FlierBehaviour { get; set; }
        public List<FlierImage> ImageList { get; set; }
        public string ExternalSource { get; set; }
        public string ExternalId { get; set; }

        public string CountryCode { get; set; }
        public string PostCode { get; set; }
        public Dictionary<string, object> ExtendedProperties { get; set; }
        public int NumberOfClaims { get; set; }
        public int NumberOfComments { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public bool UseBrowserContactDetails { get; set; }
        public HashSet<string> Boards { get; set; } 


        public HashSet<PaymentOption> PaymentOptions { get; set; }

        public HashSet<EntityFeature> Features{ get; set; }
    }

    public enum FeatureStatus
    {
        Disabled,
        Enabled
    }

    public enum FeatureType
    {
        TearOff
    }

    public class EntityFeature
    {
        public FeatureType FeatureType { get; set; }

        public FeatureStatus Status { get; set; }

        public double Cost { get; set; }
    }
}