using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;
using PostaFlya.DataRepository.Search.SearchRecord;
using PostaFlya.Domain.Flier.Event;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.Sql;
using PostaFlya.DataRepository.Binding;
using PostaFlya.Domain.Flier;
using Website.Infrastructure.Domain;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.DataRepository.Search.Implementation
{
    public class SqlFlierSearchService :  FlierSearchServiceInterface
    {
        private readonly SqlConnection _connection;
        public SqlFlierSearchService([SqlSearchConnectionString]string searchDbConnectionString)
        {
            _connection = new SqlConnection(searchDbConnectionString);
        }

        public IList<string> FindFliersByBoard(string board, int take, FlierInterface skipPast = null, Tags tags = null,
                                       FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, Location location = null, int distance = 5)
        {
            if(string.IsNullOrWhiteSpace(board))
                return new List<string>();


            var sqlCmd = _searhStringByBoard;

            //      @board uniqueidentifier,
            //		@loc geography,
            //		@top int,
            //		@distance int,
            //		@sort int = 1,
            //		@skipFlier nvarchar(255) = null,
            //		@xpath nvarchar(1000) = null

            var watch = new Stopwatch();
            watch.Start();

            var ret = SqlExecute.Query<BoardFlierSearchRecord>(sqlCmd,
                _connection
                , new object[] { new Guid(board) }
                , new
                {
                    board = new Guid(board),
                    loc = location != null && location.IsValid ? location.ToGeography() : null,
                    distance,
                    top = take,
                    sort = GetOrderByForSortOrder(sortOrder),
                    skipFlier = skipPast != null ? skipPast.Id : null,
                    xpath = GetTagFilter(tags)
                }
                    , true
                ).ToList();

            Trace.TraceInformation("FindFliers time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            //because of possible federation fan out above make sure we re-order
            //may return more than take but can't avoid that nicely
            var list = ret
                .Select(sr => sr.FlierId.ToString())
                .Distinct();

            if (take > 0)
                list = list.Take(take);

            return list.ToList();
        }

        public IList<string> FindFliersByLocationAndDistance(Location location, int distance, int take, FlierInterface skipPast = null, Tags tags = null, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate)
        {                
            if(location == null || !location.IsValid)
                return new List<string>();

            var sqlCmd = _searchString;

            var watch = new Stopwatch();
            watch.Start();

            //	@loc geography,
            //	@top int,
            //	@distance int,
            //	@sort int,
            //	@skipPastDate datetimeoffset = null,
            //	@xpath nvarchar(1000) = null

            var shards = location.GetShardIdsFor(distance).Cast<object>().ToArray();
            var ret = SqlExecute.Query<FlierSearchRecordWithDistance>(sqlCmd,
                _connection
                , shards
                , new
                {
                    loc = location.ToGeography(),
                    distance,
                    top = take,
                    sort = GetOrderByForSortOrder(sortOrder),
                    skipPastDate = skipPast == null ? (DateTimeOffset?)null : new DateTimeOffset(skipPast.CreateDate),
                    xpath = GetTagFilter(tags)
                }, true).ToList();

            Trace.TraceInformation("FindFliers time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            //because of possible federation fan out above make sure we re-order         
            var list = ret
                .OrderBy(sr => sr, SorterForOrder(sortOrder))
                .Select(sr => sr.Id.ToString())
                .Distinct();

            if (take > 0)
                list = list.Take(take);

            return list.ToList();

        }

        public IList<EntityIdInterface> IterateAllIndexedFliers(int take, FlierInterface skipPast, Tags tags = null)
        {
            var sqlCmd = _searhStringAll;
            var watch = new Stopwatch();
            watch.Start();

            //	@loc geography,
            //	@top int,
            //	@distance int,
            //	@sort int,
            //	@skipPastDate datetimeoffset = null,
            //	@xpath nvarchar(1000) = null

            var ret = SqlExecute.Query<FlierSearchRecord>(sqlCmd,
                _connection
                , new object[] {}//all shards/partitions
                , new
                    {
                        loc = (SqlGeography)null,
                        distance = 0,
                        top = take,
                        skipPastDate = skipPast == null ? (DateTimeOffset?)null : new DateTimeOffset(skipPast.CreateDate),
                        xpath = GetTagFilter(tags)

                    }, true).ToList();

            Trace.TraceInformation("IterateAllIndexedFliers time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            var list = ret
                .Distinct(new EntityIdEquals());

            if (take > 0)
                list = list.Take(take);

            return list.ToList();
        }

        private static string GetTagFilter(IEnumerable<string> tags)
        {
            if (tags == null || !tags.Any())
                return null;
            var builder = new StringBuilder();

            foreach (var tag in tags.Select(s => s.ToLowerHiphen()))
            {
                builder.Append(builder.Length == 0 ? "//tags[" : " and ");
                builder.Append("tag=\"");
                builder.Append(System.Security.SecurityElement.Escape(tag));
                builder.Append("\"");
            }
            builder.Append("]");
            return builder.ToString();
        }

        private static int GetOrderByForSortOrder(FlierSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case FlierSortOrder.CreatedDate:
                    return 1;
                case FlierSortOrder.EffectiveDate:
                    return 2;               
                case FlierSortOrder.Popularity:
                default:
                    return 4;
            }
        }

        private readonly string _searchString = Properties.Resources.SqlSearchFliersByLocationTags;
        private readonly string _searhStringByBoard = Properties.Resources.SqlSeachFliersByBoard;
        private readonly string _searhStringAll = Properties.Resources.SqlAllOrderedBy;

        private static IComparer<FlierSearchRecordWithDistance> SorterForOrder(FlierSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case FlierSortOrder.CreatedDate:
                    return new FlierSearchRecordWithDistanceComparer((x, y) =>
                    {
                        if (x.CreateDate > y.CreateDate)
                            return -1;
                        if (x.CreateDate < y.CreateDate)
                            return 1;
                        if (x.PopularityRank > y.PopularityRank)
                            return -1;
                        if (x.PopularityRank < y.PopularityRank)
                            return 1;
                        if (x.Metres < y.Metres)
                            return -1;
                        if (x.Metres > y.Metres)
                            return 1;
                        return 0;
                    });
                case FlierSortOrder.EffectiveDate:
                    return new FlierSearchRecordWithDistanceComparer((x, y) =>
                    {
                        if (x.EffectiveDate > y.EffectiveDate)
                            return -1;
                        if (x.EffectiveDate < y.EffectiveDate)
                            return 1;
                        if (x.PopularityRank > y.PopularityRank)
                            return -1;
                        if (x.PopularityRank < y.PopularityRank)
                            return 1;
                        if (x.Metres < y.Metres)
                            return -1;
                        if (x.Metres > y.Metres)
                            return 1;
                        return 0;
                    });
                case FlierSortOrder.Popularity:
                default:
                    return new FlierSearchRecordWithDistanceComparer((x, y) =>
                    {
                        if (x.PopularityRank > y.PopularityRank)
                            return -1;
                        if (x.PopularityRank < y.PopularityRank)
                            return 1;
                        if (x.Metres < y.Metres)
                            return -1;
                        if (x.Metres > y.Metres)
                            return 1;
                        if (x.CreateDate > y.CreateDate)
                            return -1;
                        if (x.CreateDate < y.CreateDate)
                            return 1;
                        return 0;
                    });
            }
        }

        private class FlierSearchRecordWithDistanceComparer : IComparer<FlierSearchRecordWithDistance>
        {
            private readonly Func<FlierSearchRecordWithDistance, FlierSearchRecordWithDistance, int> _compareFunc;

            public FlierSearchRecordWithDistanceComparer(
                Func<FlierSearchRecordWithDistance, FlierSearchRecordWithDistance, int> compareFunc)
            {
                _compareFunc = compareFunc;
            }

            public int Compare(FlierSearchRecordWithDistance x, FlierSearchRecordWithDistance y)
            {
                return _compareFunc(x, y);
            }
        }
    }
}
