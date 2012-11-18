using System;
using Microsoft.SqlServer.Types;
using PostaFlya.Domain.Boards;
using Website.Azure.Common.Sql;

namespace PostaFlya.DataRepository.Search.SearchRecord
{
    public static class BoardIntefaceSearchExtension
    {
        public static BoardSearchRecord ToSearchRecord(this BoardInterface board)
        {
            return new BoardSearchRecord()
                {
                    Location = board.Location.ToGeography(),
                    LocationShard = board.Location.GetShardId(),
                    FriendlyId = board.FriendlyId,
                    Id = board.Id
                };
        }
    }
    public class BoardSearchRecord
    {
        [PrimaryKey]
        public String Id { get; set; }

        public String FriendlyId { get; set; }

        [FederationCol(FederationName = "Location", DistributionName = "location_shard")]
        public long LocationShard { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Location { get; set; }

    }
}
