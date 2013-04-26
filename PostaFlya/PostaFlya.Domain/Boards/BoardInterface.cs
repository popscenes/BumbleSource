using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Venue;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Util;

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
            target.Name = source.Name;
        }

        //board and information need to already be close by location wise for this
        public static bool MatchVenueBoard(this BoardInterface board, VenueInformation information)
        {
            if (board.InformationSources.Any(venueInformation => venueInformation.Source == information.Source
                                                                 && venueInformation.SourceId == information.SourceId))
                return true;

            if (board.Name.LevenshteinDistanceAsPercentage(information.PlaceName) < 20)
                return true;

            return false;
        }
    }

    public interface BoardInterface : EntityInterface, BrowserIdInterface
    {
        bool AllowOthersToPostFliers { get; set; }
        bool RequireApprovalOfPostedFliers { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        BoardStatus Status { get; set; }
        BoardTypeEnum BoardTypeEnum { get; set; }
        Location Location { get; set; }
        List<VenueInformation> InformationSources { get; set; }
    }
}