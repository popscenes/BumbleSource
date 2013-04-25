using System;
using Microsoft.SqlServer.Types;
using PostaFlya.Domain.Boards;
using Website.Azure.Common.Sql;

namespace PostaFlya.DataRepository.Search.SearchRecord
{
    public static class BoardIntefaceSearchExtension
    {
        public static VenueBoardSearchRecord ToSearchRecord(this VenueBoardInterface board)
        {
            return new VenueBoardSearchRecord()
                {
                    Location = board.Location.ToGeography(),
                    LocationShard = board.Location.GetShardId(),
                    FriendlyId = board.FriendlyId,
                    Id = board.Id,
                    BoardStatus = (int) board.Status
                };
        }
    }
    public class VenueBoardSearchRecord
    {
        [PrimaryKey]
        public String Id { get; set; }

        public String FriendlyId { get; set; }

        [FederationCol(FederationName = "Location", DistributionName = "location_shard")]
        public long LocationShard { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Location { get; set; }

        public int BoardStatus { get; set; }

    }
}
