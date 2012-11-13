﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.Sql;

namespace PostaFlya.DataRepository.Search.Services
{
    public static partial class BoardInterfaceSearchExtensions
    {
        public static BoardFlierLocationSearchRecord ToSearchRecord(this BoardFlierInterface boardFlier, long locationShard)
        {
            return new BoardFlierLocationSearchRecord()
                {
                    LocationShard = locationShard,
                    FlierId = boardFlier.FlierId,
                    BoardId = boardFlier.AggregateId,
                    Id = boardFlier.Id,
                    BoardStatus = (int) boardFlier.Status
                };
        }
    }
    [Serializable]
    public class BoardFlierLocationSearchRecord
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
