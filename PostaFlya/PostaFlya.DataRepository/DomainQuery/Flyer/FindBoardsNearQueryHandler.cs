using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Indexes;
using PostaFlya.DataRepository.Search.SearchRecord;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.Sql;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Flyer
{
    public class FindBoardsNearQueryHandler : QueryHandlerInterface<FindBoardsNearQuery, List<string>>
    {
        //private readonly SqlConnection _connection;
        private readonly QueryChannelInterface _queryChannel;
        private readonly TableIndexServiceInterface _indexService;

        public FindBoardsNearQueryHandler(//[SqlSearchConnectionString]string searchDbConnectionString, 
            QueryChannelInterface queryChannel, TableIndexServiceInterface indexService)
        {
            _queryChannel = queryChannel;
            _indexService = indexService;
            //_connection = new SqlConnection(searchDbConnectionString);
        }

        private List<string> QueryIndex(FindBoardsNearQuery argument)
        {
            if (string.IsNullOrWhiteSpace(argument.Location.Id)) return null;

            var low = argument.Location.Id;
            var boards = _indexService.FindEntitiesByIndex<BoardInterface, JsonTableEntry>(
                new BoardSuburbIndex()
                , low);

            var list = (from g in boards
                        let gc = g.GetEntity<GeoPoints>()
                        let dist = gc.Distance
                        let metres = argument.WithinMetres
                        orderby dist
                        where dist <= metres
                        select g
           ).ToList();

            return list
                .Select(f => f.RowKey.ExtractEntityIdFromRowKey())
                .ToList();
        }

//        private List<string> QuerySql(FindBoardsNearQuery argument)
//        {
//            const string sqlCmd = "FindNearbyBoards";
//            var location = argument.Location;
//
//            var shards = location.GetShardIdsFor(argument.WithinMetres).Cast<object>().ToArray();
//
//            var ret = SqlExecute.Query<BoardSearchRecordWithDistance>(sqlCmd,
//                _connection
//                , shards
//                , new
//                {
//                    loc = location != null && location.IsValid() ? location.ToGeography() : null,
//                    withinmetres = argument.WithinMetres
//                }
//                    , true
//                ).ToList();
//
//            //because of possible federation fan out above make sure we re-order
//            //may return more than take but can't avoid that nicely
//            var result = ret
//                .OrderBy(distance => distance.Metres)
//                .Select(sr => sr.Id.ToString())
//                .Distinct();
//
//            if (argument.Skip > 0)
//                result = result.Skip(argument.Skip);
//
//            if (argument.Take > 0)
//                result = result.Take(argument.Take);
//
//            return result.ToList();
//        }

        public List<string> Query(FindBoardsNearQuery argument)
        {

            //return QuerySql(argument);
            return QueryIndex(argument);

        }
    }
}