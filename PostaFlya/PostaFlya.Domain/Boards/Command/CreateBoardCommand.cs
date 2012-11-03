using Website.Domain.Location;
using Website.Infrastructure.Command;

namespace PostaFlya.Domain.Boards.Command
{
    public class CreateBoardCommand : DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public string BoardName { get; set; }
        public bool AllowOthersToPostFliers { get; set; }
        public bool RequireApprovalOfPostedFliers { get; set; }
        public Location Location { get; set; }
        
    }
}