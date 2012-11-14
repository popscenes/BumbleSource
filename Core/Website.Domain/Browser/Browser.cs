using System;
using System.Collections.Generic;
using Website.Infrastructure.Domain;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace Website.Domain.Browser
{
    [Serializable]
    public class Browser : EntityBase<BrowserInterface>, BrowserInterface
    {
        public Browser(string id)
            :this()
        {
            Id = id;
        }

        public Browser()
        {
            Tags = new Tags();
            SavedTags = new List<Tags>();
            SavedLocations = new Locations();
            Roles = new Roles();
            Properties = new Dictionary<string, object>();
        }

        public Tags Tags { get; set; }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string Surname { get; set; }
        public Location.Location Address { get; set; }

        public bool AddressPublic { get; set; }
        public string AvatarImageId { get; set; }
        public int? Distance { get;set;}
        public Roles Roles { get; set; }
        public Location.Location DefaultLocation { get; set; }
        public Locations SavedLocations { get; set; }
        public List<Tags> SavedTags  { get; set; }
        [AggregateMemberEntity]
        public HashSet<BrowserIdentityProviderCredential> ExternalCredentials { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public double AccountCredit { get; set; }
    }
}