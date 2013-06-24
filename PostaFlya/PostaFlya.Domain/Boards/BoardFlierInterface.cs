using System;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards
{
    public static class BoardFlierInterfaceExtensions
    {
        public static string GetIdFor(this BoardFlierInterface source)
        {
            return source.BoardId;
        }
        public static void CopyFieldsFrom(this BoardFlierInterface target, BoardFlierInterface source)
        {
            target.Status = source.Status;
            target.BoardId = source.BoardId;
            target.BoardRank = source.BoardRank;
        }
    }

    public interface BoardFlierInterface
    {
        BoardFlierStatus Status { get; set; }
        string BoardId { get; set; }
        DateTime DateAdded { get; set; }
        int BoardRank { get; set; }
    }
}