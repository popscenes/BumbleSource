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
            Roles = new Roles();
            Properties = new Dictionary<string, object>();
        }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string Surname { get; set; }
        public Location.Location Address { get; set; }
        public string WebSite { get; set; }

        public string AvatarImageId { get; set; }
        public Roles Roles { get; set; }
        [AggregateMemberEntity]
        public HashSet<BrowserIdentityProviderCredential> ExternalCredentials { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public double AccountCredit { get; set; }
    }
}