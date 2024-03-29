using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Venue;
using Website.Domain.Contact;
using Website.Domain.Payment;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Domain;
using Website.Domain.Browser;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Domain.Tag;
using Website.Infrastructure.Query;

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


            target.Title = source.Title;
            target.Description = source.Description;
            target.Tags = new Tags(source.Tags);
            target.Image = source.Image;
            target.EffectiveDate = source.EffectiveDate;
            target.EventDates = new List<DateTimeOffset>(source.EventDates);
            target.CreateDate = source.CreateDate;
            target.FlierBehaviour = source.FlierBehaviour;
            target.Status = source.Status;
            target.ImageList = source.ImageList;
            target.ExternalSource = source.ExternalSource;
            target.ExternalId = source.ExternalId;
            target.ExtendedProperties = source.ExtendedProperties != null
                                            ? new Dictionary<string, object>(source.ExtendedProperties)
                                            : null;
            target.Boards = source.Boards != null ? new List<BoardFlier>(source.Boards) : null;
            target.HasLeadGeneration = source.HasLeadGeneration;
            target.LocationRadius = source.LocationRadius;
            target.EnableAnalytics = source.EnableAnalytics;
            target.UserLinks = source.UserLinks != null ? new List<UserLink>(source.UserLinks) : null;
        }        

        public static VenueInformationInterface GetVenueForFlier(this FlierInterface flier, QueryChannelInterface queryChannel)
        {
            var board = queryChannel.Query(new GetFlyerVenueBoardQuery() { FlyerId = flier.Id }, (Board)null);
            return board != null ? board.Venue() : null;
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

        public static int GetTotalPaid(this FlierInterface flier)
        {
            if (flier.Features == null)
                return 0;
            return flier.Features.Aggregate(0, (d, charge) => d + charge.Paid);
        }

        public static DateTimeOffset GetFirstEventDate(this FlierInterface flier)
        {
            return flier.EventDates.OrderBy(time => time).First();
        }
    }

    public interface FlierInterface : AggregateRootInterface, BrowserIdInterface, 
        ClaimableEntityInterface,
        CommentableInterface,
        EntityFeatureChargesInterface, EntityWithTinyUrlInterface
    {
        string Title { get; set; }
        string Description { get; set; }
        Tags Tags { get; set; }
        Guid? Image { get; set; }
        List<DateTimeOffset> EventDates { get; set; }

        
        [Obsolete("Use EventDates, will be removed", false)]
        DateTime EffectiveDate { get; set; }

        DateTime CreateDate { get; set; }
        FlierStatus Status { get; set; }
        FlierBehaviour FlierBehaviour { get; set; }
        List<FlierImage> ImageList { get; set; }
        string ExternalSource { get; set; }
        string ExternalId { get; set; }
        Dictionary<string, object> ExtendedProperties { get;set; }
        List<BoardFlier> Boards { get; set; }
        bool HasLeadGeneration { get; set; }
        int LocationRadius { get; set; }
        bool EnableAnalytics { get; set; }
         List<UserLink> UserLinks { get; set; }
        
    }
}