using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.Sql;

namespace PostaFlya.DataRepository.Search.Services
{
    public static class BoardInterfaceSearchExtensions
    {
        public static BoardFlierSearchRecord ToSearchRecord(this BoardFlierInterface board, long locationShard)
        {
            return new BoardFlierSearchRecord()
                {
                    LocationShard = locationShard,
                    FlierId = board.FlierId,
                    BoardId = board.AggregateId,
                    Id = board.Id,
                    BoardStatus = (int) board.Status
                };
        }
    }
    [Serializable]
    public class BoardFlierSearchRecord
    {
        [PrimaryKey]
        public String Id { get; set; }

        [FederationCol(FederationName = "Location", DistributionName = "location_shard")]
        public long LocationShard { get; set; }

        [NotNullable]
        [SqlIndex]
        public String FlierId { get; set; }
        
        [NotNullable]
        [SqlIndex]
        public String BoardId { get; set; }

        public int BoardStatus { get; set; }
    }
}
