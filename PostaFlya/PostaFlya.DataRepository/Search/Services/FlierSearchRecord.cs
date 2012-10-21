using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.SqlServer.Types;
using Website.Azure.Common.Sql;
using PostaFlya.Domain.Flier;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.DataRepository.Search.Services
{
    internal static class FlierInterfaceSearchExtensions
    {
        public static FlierSearchRecord ToSearchRecord(this FlierInterface flier)
        {
            var ret =  new FlierSearchRecord()
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
                Location = flier.Location.ToGeography(),
                LocationShard = flier.Location.GetShardId()
            };

            return ret;
        }

        public static long GetShardId(this Location loc)
        {
            return (long) (((loc.Latitude + 90)*1000) + (loc.Longitude + 180));
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
                return null;//invalid location
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
                return null;//invalid location
            }
        }
    }
    internal class FlierSearchRecord
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
