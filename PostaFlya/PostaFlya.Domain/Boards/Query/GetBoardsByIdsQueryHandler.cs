using System.Collections.Generic;
using System.Linq;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Query
{
    public class GetBoardsByIdsQueryHandler : QueryHandlerInterface<GetBoardsByIdsQuery, List<Board>>
    {
        private readonly GenericQueryServiceInterface _queryService;

        public GetBoardsByIdsQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public List<Board> Query(GetBoardsByIdsQuery argument)
        {
            return _queryService.FindByIds<Board>(argument.Ids).ToList();
        }
    }
}