using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Behaviour;
using Website.Domain.Browser;
using Website.Domain.Claims;
using Website.Domain.Contact;
using Website.Infrastructure.Domain;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;


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
            Boards = new HashSet<string>();
            Features = new HashSet<EntityFeatureInterface>();
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
        
        public Double ClaimCost(Claim claim)
        {

            var contextType = claim.ClaimContext.Split(new char[] {'|'}, StringSplitOptions.None).First();

            if (contextType == "tearoff")
            {
                if (NumberOfClaims > 0)
                {
                    return 0.00;
                }
                else
                {
                    var feature = Features.FirstOrDefault(_ => _.FeatureType == FeatureType.TearOff);
                    return feature == null ? 0.00 : feature.Cost;
                }
            }
            else
            {
                var feature = Features.FirstOrDefault(_ => _.FeatureType == FeatureType.UserContact);
                return feature == null ? 0.00 : feature.Cost;
            }

        }

        public int NumberOfComments { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public bool UseBrowserContactDetails { get; set; }
        public HashSet<string> Boards { get; set; }
        public HashSet<EntityFeatureInterface> Features { get; set; }
    }

    public enum FeatureType
    {
        TearOff,
        UserContact
    }

    public interface EntityFeatureInterface
    {
        double Cost { get; set; }
        String BrowserId { get; set; }
        FeatureType FeatureType { get; set; }
        bool IsEnabled(GenericQueryServiceInterface browserQueryService);
    }

    public class SimpleEntityFeature : EntityFeatureInterface
    {
        public double Cost { get; set; }

        public FeatureType FeatureType { get; set; }

        public bool IsEnabled(GenericQueryServiceInterface browserQueryService)
        {
            var browser = browserQueryService.FindById<Browser>(BrowserId);
            return (browser.AccountCredit >= Cost);
        }


        public string BrowserId { get; set; }
    }
}