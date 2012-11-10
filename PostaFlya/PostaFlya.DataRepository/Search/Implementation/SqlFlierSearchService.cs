using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Flier.Event;
using Website.Azure.Common.Sql;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Search.Services;
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

        public IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0)
        {
            if (distance <= 0)
                distance = 10;

            var orderbyexpress = GetOrderByForSortOrder(sortOrder);
            var xpathtags = GetTagFilter(tags);
            var gettagfilter = string.IsNullOrWhiteSpace(xpathtags) ? "" : string.Format(TagFilterTemplate, GetTagFilter(tags));
            var takeexpress = take > 0 ? "top (@take)" : "";
            var sqlCmd = string.Format(SearchString, orderbyexpress, gettagfilter, takeexpress);

            var watch = new Stopwatch();
            watch.Start();

            var loc = location.ToGeography();
            if (loc == null)
                return new List<string>();

            var ret = SqlExecute.Query<FlierSearchRecord>(sqlCmd, 
                _connection
                , new object[]{location.GetShardId()}//todo if we expand shards pass all shards where fliers may exist in here
                , new {loc, skip, take, distance });
            
            Trace.TraceInformation("FindFliers time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            return ret.Select(sr => sr.Id.ToString()).ToList();

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

        private const string TagFilterTemplate = "and Tags.exist('{0}') > 0";

        //{0} orderby
        //{1} tag filter
        //{2} take expression
        private const string SearchString = 
            @";WITH sorted AS " +
            @"( " +
            @"    SELECT  Location.STDistance(@loc) as meters, *, " +
            @"            ROW_NUMBER() OVER" +
            @"                     (ORDER BY {0}) AS RN " +
            @"    FROM    FlierSearchRecord " + 
            @"    WHERE Location.STDistance(@loc) <= @distance*1000 " +
            @"	        {1} " + 
            @") " +
            @"SELECT  {2} * " +
            @"FROM sorted " +
            @"WHERE RN > (@skip) ";


    }
}
