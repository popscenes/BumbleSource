using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.Sql;

namespace PostaFlya.DataRepository.Search.Services
{
    public static partial class BoardInterfaceSearchExtensions
    {
        public static BoardFlierSearchRecord ToSearchRecord(this BoardFlierInterface boardFlier, FlierInterface flier = null)
        {
            var ret =  new BoardFlierSearchRecord()
            {
                FlierId = boardFlier.FlierId,
                BoardId = new Guid(boardFlier.AggregateId),
                Id = boardFlier.Id,
                BoardStatus = (int)boardFlier.Status,
                BoardRank = boardFlier.BoardRank
            };

            if (flier != null)
                ret.Tags = flier.Tags.ToXml().ToSql();
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
        public String FlierId { get; set; }

        [NotNullable]
        [SqlIndex]
        [FederationCol(FederationName = "Board", DistributionName = "board_shard")]
        public Guid BoardId { get; set; }

        public DateTimeOffset DateAdded { get; set; }
        public int BoardStatus { get; set; }
        public int BoardRank { get; set; }

        //flier properties, for filtering
        public SqlXml Tags { get; set; }
    }
}
