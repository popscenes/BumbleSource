using System.Collections.Generic;
using PostaFlya.Domain.Venue;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards
{
    public static class BoardInterfaceExtensions
    {
        public static void CopyFieldsFrom(this BoardInterface target, BoardInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            BrowserIdInterfaceExtensions.CopyFieldsFrom(target, source);
            target.AllowOthersToPostFliers = source.AllowOthersToPostFliers;
            target.RequireApprovalOfPostedFliers = source.RequireApprovalOfPostedFliers;
            target.Status = source.Status;
            target.BoardTypeEnum = source.BoardTypeEnum;
            target.InformationSources = source.InformationSources != null
                                            ? new List<VenueInformation>(source.InformationSources)
                                            : null;
            target.Location = source.Location != null ? new Location(source.Location) : null;
        }
    }

    public interface BoardInterface : EntityInterface, BrowserIdInterface
    {
        bool AllowOthersToPostFliers { get; set; }
        bool RequireApprovalOfPostedFliers { get; set; }
        string Description { get; set; }
        BoardStatus Status { get; set; }
        BoardTypeEnum BoardTypeEnum { get; set; }
        Location Location { get; set; }
        List<VenueInformation> InformationSources { get; set; }
    }
}