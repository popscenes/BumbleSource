using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.Sql;

namespace PostaFlya.DataRepository.Search.SearchRecord
{
    public static partial class BoardInterfaceSearchExtensions
    {
        public static BoardFlierSearchRecord ToSearchRecord(this BoardFlierInterface boardFlier, FlierInterface flier)
        {
            var ret =  new BoardFlierSearchRecord()
            {
                FlierId = boardFlier.FlierId,
                BoardId = new Guid(boardFlier.AggregateId),
                Id = boardFlier.Id,
                BoardStatus = (int)boardFlier.Status,
                BoardRank = boardFlier.BoardRank,
            };

            if (flier == null)
                return ret;

            var geog = flier.Location.ToGeography();
            geog = geog.BufferWithTolerance(flier.LocationRadius * 1000, 0.2, false);
            ret.EffectiveDate = flier.EffectiveDate;
            ret.CreateDate = flier.CreateDate;
            ret.DateAdded = new DateTimeOffset(boardFlier.DateAdded);
            ret.Tags = flier.Tags.ToXml().ToSql();
            ret.Location = geog;
            ret.PopularityRank = flier.NumberOfComments + flier.NumberOfClaims;
            ret.NumberOfClaims = flier.NumberOfClaims;       
            return ret;
        }
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
        public int BoardRank { get; set; }

        //flier properties, for filtering

        [NotNullable]
        [SqlIndex]
        public String FlierId { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Location { get; set; }

        public int PopularityRank { get; set; }
        public int NumberOfClaims { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        [SqlIndex]
        public DateTimeOffset CreateDate { get; set; }
        
        public SqlXml Tags { get; set; }
    }
}
