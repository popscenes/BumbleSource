using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Models.Board;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Query
{
    public class GetBoardsByIdsToSummaryViewQueryHandler : QueryHandlerInterface<GetBoardsByIdsQuery, List<BoardSummaryModel>>
    {
        private readonly QueryChannelInterface _queryChannel;

        public GetBoardsByIdsToSummaryViewQueryHandler(QueryChannelInterface _queryChannel)
        {
            this._queryChannel = _queryChannel;
        }

        public List<BoardSummaryModel> Query(GetBoardsByIdsQuery argument)
        {
            var boards = _queryChannel.Query(argument, (List<Board>)null);
            return _queryChannel.ToViewModel<BoardSummaryModel, PostaFlya.Domain.Boards.Board>(boards);
        }
    }
}
