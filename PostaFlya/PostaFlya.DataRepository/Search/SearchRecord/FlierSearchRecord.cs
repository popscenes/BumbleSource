using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SqlServer.Types;
using Website.Azure.Common.Sql;
using PostaFlya.Domain.Flier;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.DataRepository.Search.SearchRecord
{
    public static class FlierInterfaceSearchExtensions
    {
        public static IEnumerable<FlierDateSearchRecord> ToDateSearchRecords(this FlierInterface flier, IEnumerable<FlierSearchRecord> searchRecords)
        {
            var recs = from sr in searchRecords
                       from ed in flier.EventDates
                       select new FlierDateSearchRecord()
                           {
                               LocationShard = sr.LocationShard,
                               Id = sr.Id,
                               EventDate = ed
                           };
            return recs;

        }
        public static IEnumerable<FlierSearchRecord> ToSearchRecords(this FlierInterface flier)
        {
            SqlGeography geog = null;
            var shards = flier.Location.GetShardIdsFor(flier.LocationRadius*1000, out geog);

            return shards.Select(shard => new  FlierSearchRecord()
                {
                    Id = flier.Id,
                    FriendlyId = flier.FriendlyId,
                    BrowserId = flier.BrowserId,
                    NumberOfClaims = flier.NumberOfClaims,
                    NumberOfComments = flier.NumberOfComments,
                    EffectiveDate = flier.EffectiveDate,
                    CreateDate = flier.CreateDate,
                    LastActivity = DateTime.UtcNow,
                    Tags = flier.Tags.ToXml().ToSql(),
                    Location = geog,
                    LocationShard = shard,
                    SortOrder = flier.CreateDate.Ticks,
                    Status = (int)flier.Status
                });
        }

        public static long[] GetShardIdsFor(this Location location, int distance)
        {
            SqlGeography geog = null;
            return location.GetShardIdsFor(distance, out geog);
        }

        public static long[] GetShardIdsFor(this Location location, int distance, out SqlGeography geog)
        {
            geog = location.ToGeography();
            var shards = new HashSet<long>();
            geog = geog.BufferWithTolerance(distance, 0.2, false);

            for (var i = 0; i < geog.STNumPoints(); i++)
            {
                var point = geog.STPointN(i + 1);
                shards.Add(point.GetShardId());
            }
            return shards.ToArray();
        }

        public static long GetShardId(this Location loc)
        {
            return (long) ((((int)loc.Latitude + 90)*1000) + ((int)loc.Longitude + 180));
        }

        public static long GetShardId(this SqlGeography loc)
        {
            return (long)((((int)loc.Lat + 90) * 1000) + ((int)loc.Long + 180));
        }

        public static XElement ToXml(this Tags tags)
        {
            var element = new XElement("tags");
            foreach (var tag in tags)
            {
                element.Add(new XElement("tag", tag.ToLowerHiphen()));
            }
            return element;
        }

        public static SqlGeography ToGeography(this Location location)
        {
            try
            {
                return SqlGeography.Point(location.Latitude, location.Longitude, SqlExecute.Srid);
            }
            catch (Exception e)
            {
                Trace.TraceError("SqlGeography ToGeography failed: {0}\n {0}", e.Message, e.StackTrace);
                return null;
            }
        }
        public static SqlGeography ToGeography(this BoundingBox boundingBox)
        {
            try
            {
                var min = SqlGeography.Point(boundingBox.Min.Latitude, boundingBox.Min.Longitude, SqlExecute.Srid);
                var max = SqlGeography.Point(boundingBox.Max.Latitude, boundingBox.Max.Longitude, SqlExecute.Srid);
                return min.STUnion(max);
            }
            catch (Exception e)
            {
                Trace.TraceError("SqlGeography ToGeography failed: {0}\n {0}", e.Message, e.StackTrace);

                return null;//invalid location
            }
        }
    }

    [Serializable]
    public class FlierDateSearchRecord
    {
        [PrimaryKey]
        public String Id { get; set; }
        [PrimaryKey]
        public DateTimeOffset EventDate { get; set; }
        //for scaling possibilities
        [FederationCol(FederationName = "Location", DistributionName = "location_shard")]
        public long LocationShard { get; set; }
    }

    public class FlierSearchRecordWithDistance : FlierSearchRecord
    {
        public double Metres { get; set; }
    }

    [Serializable]
    public class FlierSearchRecord : EntityIdInterface
    {
        [PrimaryKey]
        public String Id { get; set; }
        public String FriendlyId { get; set; }
        public String BrowserId { get; set; }
        public int NumberOfClaims { get; set; }
        public int NumberOfComments { get; set; }
        public int Status { get; set; }

        //for scaling possibilities
        [FederationCol(FederationName = "Location", DistributionName = "location_shard")]
        public long LocationShard { get; set; }

        [SqlIndex]
        public DateTimeOffset EffectiveDate { get; set; }
        [SqlIndex]
        public DateTimeOffset CreateDate { get; set; }
        
        public DateTimeOffset LastActivity { get; set; }

        public SqlXml Tags { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Location { get; set; }

        public long SortOrder { get; set; }
    }
}
