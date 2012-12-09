using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Behaviour;
using Website.Domain.Contact;
using Website.Domain.Payment;
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
        public static readonly string ClaimContextSendUserDetails = "senduserdetails";
        public static readonly string ClaimContextSendUserDetailsEnabled = "senduserdetails[paid]";
        public static readonly string ClaimContextSendUserDetailsDisabled = "senduserdetails[notpaid]";

        public static void CopyFieldsFrom(this FlierInterface target, FlierInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            BrowserIdInterfaceExtensions.CopyFieldsFrom(target, source);
            CommentableInterfaceExtensions.CopyFieldsFrom(target, source);
            ClaimableInterfaceExtensions.CopyFieldsFrom(target, source);
            EntityFeatureChargesInterfaceExtension.CopyFieldsFrom(target, source);
            target.ContactDetails = source.ContactDetails != null ? new ContactDetails() : null;
            target.ContactDetails.CopyFieldsFrom(source.ContactDetails);

            target.Title = source.Title;
            target.Description = source.Description;
            target.Tags = new Tags(source.Tags);
            target.Location = new Location(source.Location);
            target.Image = source.Image;
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
            target.UseBrowserContactDetails = source.UseBrowserContactDetails;
            target.Boards = source.Boards != null ? new HashSet<string>(source.Boards) : null;
            target.HasLeadGeneration = source.HasLeadGeneration;
            target.LocationRadius = source.LocationRadius;
        }        

        public static bool HasContactDetails(this FlierInterface flier)
        {
            return flier.UseBrowserContactDetails || flier.ContactDetails != null;
        }

        public static bool RequiresPayment(this FlierInterface flier)
        {
            return flier.HasContactDetails();
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
        CommentableInterface,
        EntityFeatureChargesInterface
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
        HashSet<string> Boards { get; set; }
        bool HasLeadGeneration { get; set; }
        int LocationRadius { get; set; }
    }
}