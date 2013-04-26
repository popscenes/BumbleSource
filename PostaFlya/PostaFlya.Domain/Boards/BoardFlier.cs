using System;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards
{
    [Serializable]
    public class BoardFlier : EntityBase<BoardFlierInterface>,
        BoardFlierInterface
    {
        public BoardFlierStatus Status { get; set; }
        public string FlierId { get; set; }
        public DateTime DateAdded { get; set; }
        public int BoardRank { get; set; }
        public string AggregateId { get; set; }
        public string AggregateTypeTag { get; set; }
    }
}