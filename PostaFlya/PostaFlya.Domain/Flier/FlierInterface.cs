using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Venue;
using Website.Domain.Contact;
using Website.Domain.Payment;
using Website.Domain.TinyUrl;
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
        SortOrder
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
            ClaimableEntityInterfaceExtensions.CopyFieldsFrom(target, source);
            EntityFeatureChargesInterfaceExtension.CopyFieldsFrom(target, source);
            TinyUrlInterfaceExtensions.CopyFieldsFrom(target, source);
            target.ContactDetails = source.ContactDetails != null ? new VenueInformation(source.ContactDetails) : null;


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
            target.Boards = source.Boards != null ? new HashSet<string>(source.Boards) : null;
            target.HasLeadGeneration = source.HasLeadGeneration;
            target.LocationRadius = source.LocationRadius;
            target.EnableAnalytics = source.EnableAnalytics;
            target.UserLinks = source.UserLinks != null ? new List<UserLink>(source.UserLinks) : null;
        }        

        public static ContactDetailsInterface GetContactDetailsForFlier(this FlierInterface flier, BrowserInterface browser)
        {
            if (flier.ContactDetails != null && flier.ContactDetails.HasEnoughForContact())
                return flier.ContactDetails;
            return browser;
        }

        public static bool HasFeatureAndIsEnabled(this FlierInterface flier, string featureDescription)
        {
            if (flier.Features == null)
                return false;
            var ret = flier.Features.SingleOrDefault(f => f.Description.Equals(featureDescription));
            if (ret == null)
                return false;
            return flier.Status == FlierStatus.Active;
        }

        public static bool HasEventDates(this FlierInterface flier)
        {
            return flier.CreateDate != flier.EffectiveDate && flier.EffectiveDate != DateTime.MinValue;
        }

        public static int GetTotalPaid(this FlierInterface flier)
        {
            if (flier.Features == null)
                return 0;
            return flier.Features.Aggregate(0, (d, charge) => d + charge.Paid);
        }
    }

    public interface FlierInterface : BrowserIdInterface, 
        ClaimableEntityInterface,
        CommentableInterface,
        EntityFeatureChargesInterface, TinyUrlInterface
    {
        string Title { get; set; }
        string Description { get; set; }
        Tags Tags { get; set; }
        Location Location { get; set; }
        Guid? Image { get; set; }
        List<DateTime> EventDates { get; set; }
        
        [Obsolete("Use EventDates, will be removed", false)]
        DateTime EffectiveDate { get; set; }

        DateTime CreateDate { get; set; }
        FlierStatus Status { get; set; }
        FlierBehaviour FlierBehaviour { get; set; }
        List<FlierImage> ImageList { get; set; }
        string ExternalSource { get; set; }
        string ExternalId { get; set; }
        Dictionary<string, object> ExtendedProperties { get;set; }
        VenueInformation ContactDetails { get; set; }
        HashSet<string> Boards { get; set; }
        bool HasLeadGeneration { get; set; }
        int LocationRadius { get; set; }
        bool EnableAnalytics { get; set; }
         List<UserLink> UserLinks { get; set; }
        
    }
}