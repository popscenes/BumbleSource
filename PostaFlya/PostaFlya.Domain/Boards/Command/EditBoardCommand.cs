using Website.Domain.Location;
using Website.Infrastructure.Command;

namespace PostaFlya.Domain.Boards.Command
{
    public class EditBoardCommand : DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public string BoardName { get; set; }
        public bool AllowOthersToPostFliers { get; set; }
        public bool RequireApprovalOfPostedFliers { get; set; }
        public Location Location { get; set; }
        public string Description { get; set; }
        public BoardStatus Status { get; set; }
        public int PercentageOfPublicFliersToShow { get; set; }
    }

    public class EditBoardCommandHandler : CommandHandlerInterface<EditBoardCommand>
    {
        public object Handle(EditBoardCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}