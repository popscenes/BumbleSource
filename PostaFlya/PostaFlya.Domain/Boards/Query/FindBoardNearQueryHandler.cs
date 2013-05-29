using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Query
{
    internal class FindBoardNearQueryHandler : QueryHandlerInterface<FindBoardsNearQuery, List<Board>>
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly GenericQueryServiceInterface _queryService;


        public FindBoardNearQueryHandler(QueryChannelInterface queryChannel, GenericQueryServiceInterface queryService)
        {
            _queryChannel = queryChannel;
            _queryService = queryService;
        }

        public List<Board> Query(FindBoardsNearQuery argument)
        {
            var ids = _queryChannel.Query(argument, (List<string>)null);

            if (ids == null || ids.Count == 0)
                return null;

            var boards = _queryService.FindByIds<Board>(ids).ToList();

            return boards.ToList();
        }
    }
}
