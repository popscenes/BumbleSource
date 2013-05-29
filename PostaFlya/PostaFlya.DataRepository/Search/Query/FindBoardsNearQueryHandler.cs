using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Search.SearchRecord;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using Website.Azure.Common.Sql;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Search.Query
{
    internal class FindBoardsNearQueryHandler : QueryHandlerInterface<FindBoardsNearQuery, List<string>>
    {
        private readonly SqlConnection _connection;

        public FindBoardsNearQueryHandler([SqlSearchConnectionString]string searchDbConnectionString)
        {
            _connection = new SqlConnection(searchDbConnectionString);
        }

        public List<string> Query(FindBoardsNearQuery argument)
        {

            const string sqlCmd = "FindNearbyBoards";
            var location = argument.Location;

            var shards = location.GetShardIdsFor(argument.WithinMetres).Cast<object>().ToArray();

            var ret = SqlExecute.Query<BoardSearchRecordWithDistance>(sqlCmd,
                _connection
                , new object[] { shards }
                , new
                {
                    loc = location != null && location.IsValid ? location.ToGeography() : null,
                    withinmetres = argument.WithinMetres
                }
                    , true
                ).ToList();

            //because of possible federation fan out above make sure we re-order
            //may return more than take but can't avoid that nicely
            var result = ret
                .OrderBy(distance => distance.Metres)
                .Select(sr => sr.Id.ToString())               
                .Distinct();

            if (argument.Skip > 0)
                result = result.Skip(argument.Skip);

            if (argument.Take > 0)
                result = result.Take(argument.Take);

            return result.ToList();

        }
    }
}