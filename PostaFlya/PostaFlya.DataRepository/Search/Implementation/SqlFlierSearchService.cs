using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace PostaFlya.DataRepository.Search.Implementation
{
    public class SqlFlierSearchService :  FlierSearchServiceInterface
    {
        private readonly SqlConnection _connection;
        public SqlFlierSearchService([SqlSearchConnectionString]string searchDbConnectionString)
        {
            _connection = new SqlConnection(searchDbConnectionString);
        }

        public IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, string board = null, int distance = 0, int minTake = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0)
        {
            if (distance <= 0)
                distance = 10;
            if (!location.IsValid && !string.IsNullOrWhiteSpace(board))
                return FindFliersByTagsAndBoard(tags, board, minTake, sortOrder, skip);
            
            if(!location.IsValid)
                return new List<string>();

            if(string.IsNullOrWhiteSpace(board))
                return FindFliersByLocationTagsAndDistanceWithoutBoard(location
                    , tags, distance, minTake, sortOrder, skip);

            return FindFliersByLocationTagsAndDistanceWithBoard(location
                , tags, board, distance, minTake, sortOrder, skip);
        }

        public IList<string> IterateAllIndexedFliers(int minTake, int nextSkip, bool returnFriendlyId = false)
        {
            const string orderbyexpress = "CreateDate"; //if fliers are being added while running iteration then they won't affect skip/take
            var sqlCmd = string.Format(_searhStringAll, orderbyexpress);
            var watch = new Stopwatch();
            watch.Start();

            var ret = SqlExecute.Query<FlierSearchRecord>(sqlCmd,
                _connection
                , new object[] {}//all shards/partitions
                , new { skip = nextSkip, take = minTake}).ToList();

            Trace.TraceInformation("IterateAllIndexedFliers time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            return ret.Select(sr => returnFriendlyId ? sr.FriendlyId : sr.Id)
                .Distinct()
                .ToList();
        }

        private IList<string> FindFliersByTagsAndBoard(Tags tags, string board, int take, FlierSortOrder sortOrder, int skip)
        {
            var orderbyexpress = GetBoardOrderByForSortOrder(sortOrder);
            var xpathtags = GetTagFilter(tags);
            var gettagfilter = string.IsNullOrWhiteSpace(xpathtags) ? "" : string.Format(TagFilterTemplate, GetTagFilter(tags));
            var takeexpress = take > 0 ? "top (@take)" : "";
            var sqlCmd = string.Format(_searhStringByBoardOnly, orderbyexpress, gettagfilter, takeexpress);

            var watch = new Stopwatch();
            watch.Start();

            var ret = SqlExecute.Query<BoardFlierSearchRecord>(sqlCmd,
                _connection
                , new object[] { new Guid(board) }
                , new { skip, take, board }).ToList();

            Trace.TraceInformation("FindFliers time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            //query will not fan out as partition is on board
            return ret.Select(sr => sr.FlierId)
                .Distinct()
                .ToList();
        }

        private IList<string> FindFliersByLocationTagsAndDistanceWithBoard(Location location, Tags tags, string board, int distance, int take, FlierSortOrder sortOrder, int skip)
        {
            var orderbyexpress = GetOrderByForSortOrder(sortOrder);
            var xpathtags = GetTagFilter(tags);
            var gettagfilter = string.IsNullOrWhiteSpace(xpathtags) ? "" : string.Format(TagFilterTemplate, GetTagFilter(tags));
            var takeexpress = take > 0 ? "top (@take)" : "";
            var sqlCmd = string.Format(_searchStringByBoard, orderbyexpress, gettagfilter, takeexpress);

            var watch = new Stopwatch();
            watch.Start();

            var loc = location.ToGeography();
            if (loc == null)
                return new List<string>();

            var ret = SqlExecute.Query<FlierSearchRecordWithDistance>(sqlCmd,
                _connection
                , location.GetShardIdsFor(distance).Cast<object>().ToArray()
                , new { loc, skip, take, distance, board }).ToList();

            Trace.TraceInformation("FindFliers time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            //because of possible federation fan out above make sure we re-order
            //may return more than take but can't avoid that nicely
            var list = ret
                .OrderBy(sr => sr, SorterForOrder(sortOrder))
                .Select(sr => sr.Id.ToString()).Distinct().ToList();
            return list;
        }

        public IList<string> FindFliersByLocationTagsAndDistanceWithoutBoard(Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0)
        {
            var orderbyexpress = GetOrderByForSortOrder(sortOrder);
            var xpathtags = GetTagFilter(tags);
            var gettagfilter = string.IsNullOrWhiteSpace(xpathtags) ? "" : string.Format(TagFilterTemplate, GetTagFilter(tags));
            var takeexpress = take > 0 ? "top (@take)" : "";
            var sqlCmd = string.Format(_searchString, orderbyexpress, gettagfilter, takeexpress);

            var watch = new Stopwatch();
            watch.Start();

            var loc = location.ToGeography();
            if (loc == null)
                return new List<string>();

            var ret = SqlExecute.Query<FlierSearchRecordWithDistance>(sqlCmd,
                _connection
                , location.GetShardIdsFor(distance).Cast<object>().ToArray()
                , new { loc, skip, take, distance }).ToList();

            Trace.TraceInformation("FindFliers time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            //because of possible federation fan out above make sure we re-order
            //may return more than take but can't avoid that nicely            
            var list = ret
                .OrderBy(sr => sr, SorterForOrder(sortOrder))
                .Select(sr => sr.Id.ToString()).Distinct().ToList();
            return list;
        }

        private static string GetTagFilter(IEnumerable<string> tags)
        {
            if (tags == null || !tags.Any())
                return "";
            var builder = new StringBuilder();

            foreach (var tag in tags)
            {
                builder.Append(builder.Length == 0 ? "//tags[" : " and ");
                builder.Append("tag=\"");
                builder.Append(System.Security.SecurityElement.Escape(tag));
                builder.Append("\"");
            }
            builder.Append("]");
            return builder.ToString();
        }

        private static string GetOrderByForSortOrder(FlierSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case FlierSortOrder.CreatedDate:
                    return "CreateDate desc, PopularityRank desc, Location.STDistance(@loc)";
                case FlierSortOrder.EffectiveDate:
                    return "EffectiveDate desc, PopularityRank desc, Location.STDistance(@loc)";               
                case FlierSortOrder.Popularity:
                default:
                    return "PopularityRank desc, Location.STDistance(@loc), CreateDate desc";
            }
        }

        private static string GetBoardOrderByForSortOrder(FlierSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case FlierSortOrder.CreatedDate:
                case FlierSortOrder.EffectiveDate:
                    return "DateAdded desc, BoardRank";
                case FlierSortOrder.Popularity:
                default:
                    return "BoardRank, DateAdded desc";
            }
        }

        private const string TagFilterTemplate = "and Tags.exist('{0}') > 0";

        //{0} orderby
        //{1} tag filter
        //{2} take/top expression
        private readonly string _searchString = Properties.Resources.SqlSearchFliersByLocationTags;
        private readonly string _searchStringByBoard = Properties.Resources.SqlSearchFliersByBoardLocationTags;
        private readonly string _searhStringByBoardOnly = Properties.Resources.SqlSeachFliersByBoard;
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
