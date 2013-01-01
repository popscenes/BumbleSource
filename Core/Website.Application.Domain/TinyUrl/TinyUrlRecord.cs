using System;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Domain;

namespace Website.Application.Domain.TinyUrl
{
    public static class TinyUrlRecordInterfaceExtensions
    {
        public static void CopyFieldsFrom(this TinyUrlRecordInterface target, TinyUrlRecordInterface source)
        {
            EntityIdInterfaceExtensions.CopyFieldsFrom(target, source);
            AggregateInterfaceExtensions.CopyFieldsFrom(target, source);
        }
    }

    public interface TinyUrlRecordInterface : EntityIdInterface, AggregateInterface
    {
    }

    public class TinyUrlRecord : TinyUrlRecordInterface, TinyUrlInterface
    {
        public string AggregateId { get; set; }
        public string AggregateTypeTag { get; set; }
        public string Id { get; set; }
        public string FriendlyId { get; set; }
        public string TinyUrl { get; set; }
    }
}
