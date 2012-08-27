using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Behaviour;
using WebSite.Infrastructure.Domain;
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

    }
}