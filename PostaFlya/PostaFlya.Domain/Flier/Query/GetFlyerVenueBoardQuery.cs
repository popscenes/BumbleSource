using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.Domain.Boards;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public class GetFlyerVenueBoardQuery : QueryInterface<Board>
    {
        public String FlyerId { get; set; }
    }

    public class GetFlyerVenueBoardQueryHandler : QueryHandlerInterface<GetFlyerVenueBoardQuery, Board>
    {
        private readonly QueryChannelInterface _queryChannel;

        public GetFlyerVenueBoardQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public Board Query(GetFlyerVenueBoardQuery argument)
        {
            var flyer = _queryChannel.Query(new FindByIdQuery<Flier>() { Id = argument.FlyerId }, (Flier)null);
            if (flyer == null) return null;
            var boards = _queryChannel.Query(new FindByIdsQuery<Board>() { Ids = flyer.Boards.Select(b => b.BoardId) }, new List<Board>());
            return boards.FirstOrDefault(board => board.Venue() != null);
        }
    }
}
