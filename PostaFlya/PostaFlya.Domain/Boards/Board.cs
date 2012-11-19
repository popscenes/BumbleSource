using Website.Domain.Location;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards
{
    public class Board : EntityBase<BoardInterface>, BoardInterface
    {
        public string BrowserId { get; set; }
        public bool AllowOthersToPostFliers { get; set; }
        public bool RequireApprovalOfPostedFliers { get; set; }
        public Location Location { get; set; }
        public string Description { get; set; }
        public BoardStatus Status { get; set; }
    }
}