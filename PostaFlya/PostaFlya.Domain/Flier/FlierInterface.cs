using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Behaviour;
using Website.Domain.Contact;
using Website.Infrastructure.Domain;
using Website.Domain.Browser;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.Domain.Flier
{
    public enum FlierSortOrder
    {
        CreatedDate,
        EffectiveDate,
        Popularity
    }
    public static class FlierInterfaceExtensions
    {
        public static void CopyFieldsFrom(this FlierInterface target, FlierInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            target.Title = source.Title;
            target.Description = source.Description;
            target.Tags = new Tags(source.Tags);
            target.Location = new Location(source.Location);
            target.Image = source.Image;
            target.BrowserId = source.BrowserId;
            target.EffectiveDate = source.EffectiveDate;
            target.CreateDate = source.CreateDate;
            target.FlierBehaviour = source.FlierBehaviour;
            target.Status = source.Status;
            target.ImageList = source.ImageList;
            target.ExternalSource = source.ExternalSource;
            target.ExternalId = source.ExternalId;
            target.ExtendedProperties = source.ExtendedProperties != null
                                            ? new Dictionary<string, object>(source.ExtendedProperties)
                                            : null;
            target.NumberOfComments = source.NumberOfComments;
            target.NumberOfClaims = source.NumberOfClaims;
            target.UseBrowserContactDetails = source.UseBrowserContactDetails;
            target.ContactDetails = source.ContactDetails;
        }        

        public static bool HasContactDetails(this FlierInterface flier)
        {
            return flier.UseBrowserContactDetails || flier.ContactDetails != null;
        }

        public static ContactDetailsInterface GetContactDetailsForFlier(this FlierInterface flier, BrowserInterface browser)
        {
            return flier.UseBrowserContactDetails ? 
                browser as ContactDetailsInterface : 
                flier.ContactDetails;
        }
    }
    public interface FlierInterface : 
        EntityInterface, 
        BrowserIdInterface, 
        ClaimableInterface,
        CommentableInterface
    {
        string Title { get; set; }
        string Description { get; set; }
        Tags Tags { get; set; }
        Location Location { get; set; }
        Guid? Image { get; set; }
        DateTime EffectiveDate { get; set; }
        DateTime CreateDate { get; set; }
        FlierStatus Status { get; set; }
        FlierBehaviour FlierBehaviour { get; set; }
        List<FlierImage> ImageList { get; set; }
        string ExternalSource { get; set; }
        string ExternalId { get; set; }
        string CountryCode { get; set; }
        string PostCode { get; set; }
        Dictionary<string, object> ExtendedProperties { get;set; }
        ContactDetails ContactDetails { get; set; }
        bool UseBrowserContactDetails { get; set; }
    }
}