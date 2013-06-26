using System;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Board;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Query
{
    public class GetFlyerVenueBoardQueryHandler : QueryHandlerInterface<GetFlyerVenueBoardQuery, BoardSummaryModel>
    {
        private readonly QueryChannelInterface _queryChannel;

        public GetFlyerVenueBoardQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public BoardSummaryModel Query(GetFlyerVenueBoardQuery argument)
        {
            var ret = _queryChannel.Query(argument, (Board) null);
            return ret == null ? null : _queryChannel.ToViewModel<BoardSummaryModel, Board>(ret);
        }
    }
}