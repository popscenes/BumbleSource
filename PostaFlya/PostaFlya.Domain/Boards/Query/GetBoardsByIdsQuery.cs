using System.Collections.Generic;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Query
{
    public class GetBoardsByIdsQuery : QueryInterface
    {
        public IEnumerable<string> Ids { get; set; }
    }
}