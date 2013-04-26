using System;
using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace PostaFlya.Domain.Boards.Command
{
    [Serializable]
    public class AddFlierToBoardCommand : DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public IEnumerable<BoardFlier> BoardFliers { get; set; }
    }
}