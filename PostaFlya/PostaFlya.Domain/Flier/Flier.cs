using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Domain;


namespace PostaFlya.Domain.Flier
{
    [Serializable]
    public class Flier : EntityBase<FlierInterface>, FlierInterface
    {

        public Flier()
        {
            Id = Guid.NewGuid().ToString();
            Tags = new Tags();
            ExtendedProperties = new PropertyGroupCollection();
            ImageList = new List<FlierImage>();
            CreateDate = DateTime.UtcNow;
        }

        public Flier(Location.Location location)
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

        public Location.Location Location { get; set; }

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

        public int NumberOfLikes
        {
            get { return (int)(this["Like", "NumberOfLikes"] ?? 0); } 
            set { this["Like", "NumberOfLikes"] = value; }
        }

        public int NumberOfComments
        {
            get { return (int)(this["Comment", "NumberOfComments"] ?? 0); }
            set { this["Comment", "NumberOfComments"] = value; }
        }

    }
}