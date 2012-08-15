using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Browser
{
    public static class BrowserInterfaceExtensions
    {
        public static void CopyFieldsFrom(this BrowserInterface target, BrowserInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            target.Tags = new Tags(source.Tags);
            target.Handle = source.Handle;
            target.EmailAddress = source.EmailAddress;
            target.Distance = source.Distance;
            target.Roles = new Roles(source.Roles);
            target.DefaultLocation = source.DefaultLocation;
            target.SavedLocations = new Locations(source.SavedLocations.Select(l => new Location.Location(l)));
            target.SavedTags = new List<Tags>(source.SavedTags.Select(t => new Tags(t)));
            target.ExternalCredentials = source.ExternalCredentials != null ? new HashSet<IdentityProviderCredential>(source.ExternalCredentials) : null;
            target.FirstName = source.FirstName;
            target.MiddleNames = source.MiddleNames;
            target.Surname = source.Surname;
            target.Address = source.Address != null ? new Location.Location(source.Address) : null;
            target.AddressPublic = source.AddressPublic;
            target.AvatarImageId = source.AvatarImageId;
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
    }

    public interface BrowserInterface : EntityInterface
    {
        Tags Tags { get; set; }
        String Handle { get; set; }
        String EmailAddress { get; set; }
        int? Distance { get; set; }
        Roles Roles { get; set; }
        Location.Location DefaultLocation { get; set; }
        Locations SavedLocations { get; set; }
        IList<Tags> SavedTags { get; set; }
        ISet<IdentityProviderCredential> ExternalCredentials { get; set; }
        string FirstName { get; set; }
        string MiddleNames { get; set; }
        string Surname { get; set; }
        Location.Location Address { get; set; }
        string AvatarImageId { get; set; }
        bool AddressPublic { get; set; }
    }
}