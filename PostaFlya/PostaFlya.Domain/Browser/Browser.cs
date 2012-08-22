using System;
using System.Collections.Generic;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Browser
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
        }

        public Tags Tags { get; set; }

        public string Handle { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string Surname { get; set; }
        public Location.Location Address { get; set; }
        public bool AddressPublic { get; set; }
        public string EmailAddress { get; set; }
        public string AvatarImageId { get; set; }
        public int? Distance { get;set;}
        public Roles Roles { get; set; }
        public Location.Location DefaultLocation { get; set; }
        public Locations SavedLocations { get; set; }
        public List<Tags> SavedTags  { get; set; }
        [AggregateMemberEntity]
        public HashSet<BrowserIdentityProviderCredential> ExternalCredentials { get; set; }
    }
}