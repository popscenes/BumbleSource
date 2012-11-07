using Website.Infrastructure.Command;

namespace PostaFlya.Domain.Boards.Command
{
    public class EditBoardFlierCommand : DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public string FlierId { get; set; }
        public string BoardId { get; set; }
        public BoardFlierStatus Status { get; set; }
    }
}