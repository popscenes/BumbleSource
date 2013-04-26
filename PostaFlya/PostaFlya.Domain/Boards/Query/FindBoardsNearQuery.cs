using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Query
{
    public class FindBoardsNearQuery : QueryInterface
    {
        public Location Location { get; set; }
        public int WithinMetres { get; set; }
    }
}