using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.SqlServer.Types;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.Sql;
using Website.Azure.Common.TableStorage;

namespace PostaFlya.DataRepository.Search.SearchRecord
{
    public static partial class BoardInterfaceSearchExtensions
    {
        public static IEnumerable<BoardFlierDateSearchRecord> ToBoardDateSearchRecords(this BoardFlierSearchRecord searchRecord, FlierInterface flier) 
        {
            var recs = from ed in flier.EventDates
                       select new BoardFlierDateSearchRecord()
                       {
                           BoardId = searchRecord.BoardId,
                           Id = searchRecord.Id,
                           EventDate = ed,
                           SortOrder = ed.GetTimestampAscending() + flier.CreateDate.GetTimestampAscending(),
                       };
            return recs;

        }

        public static BoardFlierSearchRecord ToSearchRecord(this BoardFlierInterface boardFlier, FlierInterface flier)
        {
            var ret =  new BoardFlierSearchRecord()
            {
                FlierId = flier.Id,
                BoardId = new Guid(boardFlier.BoardId),
                Id = flier.Id + boardFlier.BoardId,
                BoardStatus = (int)boardFlier.Status,
            };

            if (flier == null)
                return ret;

            var geog = flier.Venue.Address.ToGeography();
            geog = geog.BufferWithTolerance(flier.LocationRadius * 1000, 0.2, false);
            ret.EffectiveDate = flier.EffectiveDate;
            ret.CreateDate = flier.CreateDate;
            ret.DateAdded = new DateTimeOffset(boardFlier.DateAdded);
            ret.Tags = flier.Tags.ToXml().ToSql();
            ret.Location = geog;
            ret.NumberOfClaims = flier.NumberOfClaims;
            ret.SortOrder = flier.CreateDate.Ticks;
            ret.Status = (int) flier.Status;
            return ret;
        }
    }

    [Serializable]
    public class BoardFlierDateSearchRecord
    {
        [PrimaryKey]
        public String Id { get; set; }
        [PrimaryKey]
        public DateTimeOffset EventDate { get; set; }
        //for scaling possibilities
        [NotNullable]
        [FederationCol(FederationName = "Board", DistributionName = "board_shard")]
        public Guid BoardId { get; set; }

        public string SortOrder { get; set; }
    }

    [Serializable]
    public class BoardFlierSearchRecord
    {
        [PrimaryKey]
        public string Id { get; set; }

        [NotNullable]
        [SqlIndex]
        [FederationCol(FederationName = "Board", DistributionName = "board_shard")]
        public Guid BoardId { get; set; }

        [SqlIndex]
        public DateTimeOffset DateAdded { get; set; }
        public int BoardStatus { get; set; }

        //flier properties, for filtering

        [NotNullable]
        [SqlIndex]
        public String FlierId { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Location { get; set; }

        public int NumberOfClaims { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        [SqlIndex]
        public DateTimeOffset CreateDate { get; set; }
        
        public SqlXml Tags { get; set; }

        public long SortOrder { get; set; }

        public int Status { get; set; }
    }
}
