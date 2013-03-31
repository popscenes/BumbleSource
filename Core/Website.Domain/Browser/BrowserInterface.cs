using System;
using System.Collections.Generic;
using System.Linq;
using Website.Domain.Contact;
using Website.Domain.Payment;
using Website.Infrastructure.Domain;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace Website.Domain.Browser
{
    public static class BrowserInterfaceExtensions
    {
        public static void CopyFieldsFrom(this BrowserInterface target, BrowserInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            ContactDetailsInterfaceExtensions.CopyFieldsFrom(target, source);
            target.Tags = new Tags(source.Tags);
            target.Distance = source.Distance;
            target.Roles = new Roles(source.Roles);
            target.DefaultLocation = source.DefaultLocation;
            target.SavedLocations = new Locations(source.SavedLocations.Select(l => new Location.Location(l)));
            target.SavedTags = new List<Tags>(source.SavedTags.Select(t => new Tags(t)));
            target.ExternalCredentials = source.ExternalCredentials != null ? new HashSet<BrowserIdentityProviderCredential>(source.ExternalCredentials) : null;
            target.AvatarImageId = source.AvatarImageId;
            target.Properties = source.Properties != null ? new Dictionary<string, object>(source.Properties) : null;
            target.AccountCredit = source.AccountCredit;
        }

        public static bool HasRole(this BrowserInterface browser, Role role)
        {
            return browser.Roles.Contains(role.ToString());
        }

        public static bool HasRole(this BrowserInterface browser, string role)
        {
            return browser.Roles.Contains(role);
        }

        public static bool HasAnyRole(this BrowserInterface browser, string[] roles)
        {
            return roles.Any(role => browser.Roles.Contains(role));
        }

        public static string GetNameString(this BrowserInterface browser)
        {
            return browser.FirstName + " " + browser.Surname;
        }


        public static bool IsTemporary(this BrowserInterface browser)
        {
            return browser.HasRole(Role.Temporary);
        }

        public static bool IsOwner(this BrowserInterface browser, BrowserIdInterface entity)
        {
            return browser.Id.Equals(entity.BrowserId);
        }
    }

    public interface BrowserInterface : EntityInterface, ContactDetailsInterface, ChargableEntityInterface
    {
        Tags Tags { get; set; }
        int? Distance { get; set; }
        Roles Roles { get; set; }
        Location.Location DefaultLocation { get; set; }
        Locations SavedLocations { get; set; }
        List<Tags> SavedTags { get; set; }  
        HashSet<BrowserIdentityProviderCredential> ExternalCredentials { get; set; }
        
        string AvatarImageId { get; set; }
        Dictionary<string, object> Properties { get; set; }
    }
}