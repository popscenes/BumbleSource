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
            target.Name = source.Name;
            target.ImageId = source.ImageId;
            target.DefaultInformationSource = source.DefaultInformationSource;
            target.BoardTypeEnum = source.BoardTypeEnum;
            target.AdminEmailAddresses = new List<string>(source.AdminEmailAddresses ?? new List<string>());
            target.ImageId = source.ImageId;
            target.BackgroundImageId = source.BackgroundImageId;
        }

        //board and information need to already be close by location wise for this
        public static bool MatchVenueBoard(this BoardInterface board, VenueInformation information)
        {
            if (board == null)
                return false;

            if (board.InformationSources != null && board.InformationSources.Any(venueInformation => venueInformation.Source == information.Source
                                                                 && venueInformation.SourceId == information.SourceId))
                return true;

            if (board.Name.LevenshteinDistanceAsPercentage(information.PlaceName) < 20)
                return true;

            return false;
        }

        public static VenueInformation Venue(this BoardInterface board)
        {
            if (board.InformationSources == null || board.InformationSources.Count == 0 || board.BoardTypeEnum == BoardTypeEnum.InterestBoard)
                return null;

            var ret =
                board.InformationSources.FirstOrDefault(
                    information => information.Source == board.DefaultInformationSource);
            return ret ?? board.InformationSources.First();
        }

    }

    public interface BoardInterface : EntityInterface, AggregateRootInterface, BrowserIdInterface
    {
        bool AllowOthersToPostFliers { get; set; }
        bool RequireApprovalOfPostedFliers { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        BoardStatus Status { get; set; }
        BoardTypeEnum BoardTypeEnum { get; set; }
        BoardSubscription Subscription { get; set; }
        string ImageId { get; set; }
        string BackgroundImageId { get; set; }
        List<VenueInformation> InformationSources { get; set; }
        string DefaultInformationSource { get; set; }
        List<string> AdminEmailAddresses { get; set; } 
    }

    public enum BoardSubscription
    {
        Free,
        Basic
    }
}