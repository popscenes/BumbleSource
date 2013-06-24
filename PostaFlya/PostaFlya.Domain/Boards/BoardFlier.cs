using System;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards
{
    [Serializable]
    public class BoardFlier : BoardFlierInterface
    {

        public BoardFlierStatus Status { get; set; }
        public string BoardId { get; set; }
        public DateTime DateAdded { get; set; }
        public int BoardRank { get; set; }
    }
}