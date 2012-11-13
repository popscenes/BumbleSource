using System;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards
{
    public static class BoardFlierInterfaceExtensions
    {
        public static string GetIdFor(this BoardFlierInterface source)
        {
            return source.FlierId + source.AggregateId;
        }
        public static void CopyFieldsFrom(this BoardFlierInterface target, BoardFlierInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            AggregateInterfaceExtensions.CopyFieldsFrom(target, source);
            target.Status = source.Status;
            target.FlierId = source.FlierId;
        }
    }

    public interface BoardFlierInterface : EntityInterface, AggregateInterface
    {
        BoardFlierStatus Status { get; set; }
        string FlierId { get; set; }
        DateTime DateAdded { get; set; }
    }
}