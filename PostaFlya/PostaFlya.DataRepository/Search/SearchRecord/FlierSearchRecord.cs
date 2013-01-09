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

namespace PostaFlya.DataRepository.Search.SearchRecord
{
    public static class FlierInterfaceSearchExtensions
    {
        public static IEnumerable<FlierSearchRecord> ToSearchRecords(this FlierInterface flier)
        {
            var geog = flier.Location.ToGeography();
            var shards = new HashSet<long>();
            geog = geog.BufferWithTolerance(flier.LocationRadius, 0.2, false);

            for (var i = 0; i < geog.STNumPoints(); i++)
            {
                var point = geog.STPointN(i + 1);
                shards.Add(point.GetShardId());
            }

            return shards.Select(shard => new  FlierSearchRecord()
                {
                    Id = flier.Id,
                    BrowserId = flier.BrowserId,
                    PopularityRank = flier.NumberOfComments + flier.NumberOfClaims,
                    NumberOfClaims = flier.NumberOfClaims,
                    NumberOfComments = flier.NumberOfComments,
                    EffectiveDate = flier.EffectiveDate,
                    CreateDate = flier.CreateDate,
                    LastActivity = DateTime.UtcNow,
                    Tags = flier.Tags.ToXml().ToSql(),
                    Location = geog,
                    LocationShard = shard
                });
        }

        public static long[] GetShardIdsFor(this Location location, int distance)
        {
            var geog = location.ToGeography();
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
                element.Add(new XElement("tag", tag));
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
    public class FlierSearchRecord
    {
        [PrimaryKey]
        public String Id { get; set; }
        public String BrowserId { get; set; }
        public int PopularityRank { get; set; }
        public int NumberOfClaims { get; set; }
        public int NumberOfComments { get; set; }

        //for scaling possibilities
        [FederationCol(FederationName = "Location", DistributionName = "location_shard")]
        public long LocationShard { get; set; }

        public DateTimeOffset EffectiveDate { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset LastActivity { get; set; }

        public SqlXml Tags { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Location { get; set; }
    }
}
