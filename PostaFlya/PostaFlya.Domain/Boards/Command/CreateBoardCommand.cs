using PostaFlya.Domain.Venue;
using Website.Domain.Location;
using Website.Infrastructure.Command;

namespace PostaFlya.Domain.Boards.Command
{
    public class CreateBoardCommand : DefaultCommandBase
    {
        public BoardTypeEnum BoardTypeEnum { get; set; }
        public string BrowserId { get; set; }
        public string BoardName { get; set; }
        public bool AllowOthersToPostFliers { get; set; }
        public bool RequireApprovalOfPostedFliers { get; set; }
        public Location Location { get; set; }
        public string Description { get; set; }
        public int PercentageOfPublicFliersToShow { get; set; }
        public VenueInformation SourceInformation { get; set; }
        public string FlierIdToAddOnCreate { get; set; }
    }
}