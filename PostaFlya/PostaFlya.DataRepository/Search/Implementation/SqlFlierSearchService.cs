﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;
using PostaFlya.DataRepository.Search.SearchRecord;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.Sql;
using PostaFlya.DataRepository.Binding;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Domain;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.DataRepository.Search.Implementation
{
    public class SqlFlierSearchService :  FlierSearchServiceInterface
    {
        private readonly SqlConnection _connection;
        private readonly QueryChannelInterface _queryChannel;
        public SqlFlierSearchService([SqlSearchConnectionString]string searchDbConnectionString, QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
            _connection = new SqlConnection(searchDbConnectionString);
        }

        public IList<string> FindFliersByBoard(string board, int take, FlierInterface skipPast = null, DateTime? date = null, Tags tags = null,
                                       FlierSortOrder sortOrder = FlierSortOrder.SortOrder, Location location = null, int distance = 5)
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
            //		@xpath nvarchar(1000) = null,
            //	    @eventDate datetime2 = null

            var watch = new Stopwatch();
            watch.Start();

            var sortSkipByEventDate = (skipPast == null || date == null)
                                          ? null
                                          : skipPast.EventDates.First(_ => _.Ticks >= date.Value.Ticks)
                                                    .GetTimestampAscending() +
                                            skipPast.CreateDate.GetTimestampAscending();

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
                    xpath = GetTagFilter(tags),
                    eventDate = date,
                    skipPastEventAndCreateDate = sortSkipByEventDate
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

        public IList<string> FindFliersByLocationAndDistance(Location location, int distance, int take, FlierInterface skipPast = null, Tags tags = null, DateTime? date = null, FlierSortOrder sortOrder = FlierSortOrder.SortOrder)
        {                
            var sqlCmd = _searchString;

            var watch = new Stopwatch();
            watch.Start();

            //	@loc geography,
            //	@top int,
            //	@distance int,
            //	@sort int,
            //	@skipPast bigint = null,
            //	@xpath nvarchar(1000) = null
            //	@eventDate datetime2 = null

            var venueBoard = skipPast == null
                                 ? null
                                 : _queryChannel.Query(new GetFlyerVenueBoardQuery() { FlyerId = skipPast.Id }, (Board)null);

            var hasLocation = location != null && location.IsValid;
            var sortSkip = skipPast == null ? (long?)null : skipPast.ToSearchRecords(venueBoard).First().SortOrder;
            var sortSkipByEventDate = (skipPast == null || date == null)
                                          ? null
                                          : skipPast.EventDates.First(_ => _.Ticks >= date.Value.Ticks)
                                                    .GetTimestampAscending() +
                                            skipPast.CreateDate.GetTimestampAscending();

            var shards = location.GetShardIdsFor(distance * 1000).Cast<object>().ToArray();
            var ret = SqlExecute.Query<FlierSearchRecordWithDistance>(sqlCmd,
                _connection
                , shards
                , new
                {
                    loc = !hasLocation ? (SqlGeography)null : location.ToGeography(),
                    distance = !hasLocation ? 0 : distance,
                    top = take,
                    sort = GetOrderByForSortOrder(sortOrder),
                    skipPast = sortSkip,
                    skipPastEventAndCreateDate = sortSkipByEventDate,
                    xpath = GetTagFilter(tags),
                    eventDate = date
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
            //	@skipPast bigint = null,
            //	@xpath nvarchar(1000) = null

            var venueBoard = skipPast == null
                     ? null
                     :  _queryChannel.Query(new GetFlyerVenueBoardQuery() { FlyerId = skipPast.Id }, (Board)null);

            var sortSkip = skipPast == null ? (long?)null : skipPast.ToSearchRecords(venueBoard).First().SortOrder;

            var ret = SqlExecute.Query<FlierSearchRecord>(sqlCmd,
                _connection
                , new object[] {}//all shards/partitions
                , new
                    {
                        loc = (SqlGeography)null,
                        distance = 0,
                        top = take,
                        skipPast = sortSkip,
                        xpath = GetTagFilter(tags)

                    }, true).ToList();

            Trace.TraceInformation("IterateAllIndexedFliers time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            var list = ret
                .Distinct(new EntityIdEquals());

            if (take > 0)
                list = list.Take(take);

            return list.ToList();
        }

        public IList<string> FindFlyersByBoard(string board, DateTime startdate, DateTime enddate, Tags tags = null)
        {
            var sqlCmd = _searchStringBoardDate;

            var watch = new Stopwatch();
            watch.Start();


            var ret = SqlExecute.Query<FlierSearchRecordWithDistance>(sqlCmd,
                _connection
                , new object[] { new Guid(board) }
                , new
                {
                    board = new Guid(board),
                    xpath = GetTagFilter(tags),
                    startdate = startdate,
                    enddate = enddate,
                    isUtc = startdate.Kind == DateTimeKind.Utc
                }, true).ToList();

            Trace.TraceInformation("FindFliers by board date time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());

            var list = ret
                .Select(sr => sr.Id.ToString())
                .Distinct();

            return list.ToList();
        }


        public IList<string> FindFliersByLocationAndDate(Location location, int distance, DateTime startdate, DateTime enddate,
                                                 Tags tags = null)
        {
            var sqlCmd = _searchStringDate;

            var watch = new Stopwatch();
            watch.Start();


            if (distance == 0) distance = 15;

            var shards = location.GetShardIdsFor(distance * 1000).Cast<object>().ToArray();
            var ret = SqlExecute.Query<FlierSearchRecordWithDistance>(sqlCmd,
                _connection
                , shards
                , new
                {
                    loc = location.ToGeography(),
                    distance = distance,
                    xpath = GetTagFilter(tags),
                    startdate = startdate,
                    enddate = enddate,
                    isUtc = startdate.Kind == DateTimeKind.Utc
                }, true).ToList();

            Trace.TraceInformation("FindFliers by date time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            
            var list = ret
                .Select(sr => sr.Id.ToString())
                .Distinct();

            return list.ToList();
        }

        private static string GetTagFilter(IEnumerable<string> tags)
        {
            if (tags == null || !tags.Any())
                return null;
            var builder = new StringBuilder();

            foreach (var tag in tags.Select(s => s.ToLowerHiphen()))
            {
                builder.Append(builder.Length == 0 ? "//tags[" : " or ");
                builder.Append("tag=\"");
                builder.Append(System.Security.SecurityElement.Escape(tag));
                builder.Append("\"");
            }
            builder.Append("]");
            return builder.ToString();
        }

        private static int GetOrderByForSortOrder(FlierSortOrder sortOrder)
        {
            return 1;

        }

        private readonly string _searchString = Properties.Resources.SqlSearchFliersByLocationTags;
        private readonly string _searhStringByBoard = Properties.Resources.SqlSeachFliersByBoard;
        private readonly string _searhStringAll = Properties.Resources.SqlAllOrderedBy;
        private readonly string _searchStringDate = Properties.Resources.SqlSeachFlyersByDate;
        private readonly string _searchStringBoardDate = Properties.Resources.SqlSearchFlyersByByBoardAndDate;

        private static IComparer<FlierSearchRecordWithDistance> SorterForOrder(FlierSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case FlierSortOrder.SortOrder:
                default:
                    return new FlierSearchRecordWithDistanceComparer((x, y) =>
                    {
                        if (x.SortOrderString != null && y.SortOrderString != null)
                        {
                            var val = System.String.Compare(x.SortOrderString, y.SortOrderString, System.StringComparison.Ordinal);
                            if (val != 0)
                                return val < 0 ? -1 : 1;    
                        }

                        if (x.SortOrder > y.SortOrder)
                            return -1;
                        if (x.SortOrder < y.SortOrder)
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
