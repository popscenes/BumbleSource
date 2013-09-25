using System;
using System.Linq;
using Microsoft.SqlServer.Types;
using PostaFlya.Domain.Boards;
using Website.Azure.Common.Sql;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Sharding;

namespace PostaFlya.DataRepository.Search.SearchRecord
{
    public static class BoardIntefaceSearchExtension
    {
        public static BoardSearchRecord ToSearchRecord(this BoardInterface board)
        {
            return new BoardSearchRecord()
                {
                    Location = board.InformationSources.First().Address.ToGeography(),
                    LocationShard = board.InformationSources.First().Address.GetShardId(),
                    FriendlyId = board.FriendlyId,
                    Id = board.Id,
                    BoardStatus = (int) board.Status
                };
        }
    }

    public class BoardSearchRecordWithDistance : BoardSearchRecord
    {
        public double Metres { get; set; }
    }
    public class BoardSearchRecord
    {
        [PrimaryKey]
        public String Id { get; set; }

        public String FriendlyId { get; set; }

        [FederationColumn(FederationName = "Location", DistributionName = "location_shard")]
        public long LocationShard { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Location { get; set; }

        public int BoardStatus { get; set; }

        public int SortOrder { get; set; }

    }
}
