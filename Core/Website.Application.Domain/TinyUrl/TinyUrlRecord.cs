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
            TinyUrlInterfaceExtensions.CopyFieldsFrom(target, source);
        }
    }

    public interface TinyUrlRecordInterface : EntityIdInterface, AggregateInterface, TinyUrlInterface
    {
    }

    public class TinyUrlRecord : TinyUrlRecordInterface
    {
        public const string UnassignedToAggregateId = "unassigned";
        public static string GenerateIdFromUrl(string url)
        {
            var uri = new Uri(url);
            return uri.Host + uri.AbsolutePath.Replace('/', '-');
        }

        public string AggregateId { get; set; }
        public string AggregateTypeTag { get; set; }
        public string Id { get; set; }
        public string FriendlyId { get; set; }
        public string TinyUrl { get; set; }
        
    }
}
