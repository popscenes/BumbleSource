using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Venue;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Query
{
    public class FindBoardForVenueQuery : QueryInterface
    {
        public VenueInformation VenueInformation { get; set; }
    }

    internal class FindBoardForVenueQueryHandler : QueryHandlerInterface<FindBoardForVenueQuery, Board>
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly GenericQueryServiceInterface _queryService;

        public FindBoardForVenueQueryHandler(QueryChannelInterface queryChannel
            , GenericQueryServiceInterface queryService)
        {
            _queryChannel = queryChannel;
            _queryService = queryService;
        }

        public Board Query(FindBoardForVenueQuery argument)
        {
           var ids =  _queryChannel.Query(new FindBoardsNearQuery()
               {
                   Location = argument.VenueInformation.Address,
                   WithinMetres = 25
               }, (List<string>) null);

            if (ids == null || ids.Count == 0)
                return null;
            
            var boards = _queryService.FindByIds<Board>(ids);

            var ret = boards.FirstOrDefault(board => board.MatchVenueBoard(argument.VenueInformation));

            
            return ret ?? null;
        }
    }
}