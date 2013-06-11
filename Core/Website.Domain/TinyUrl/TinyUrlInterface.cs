using System;
using Website.Infrastructure.Domain;

namespace Website.Domain.TinyUrl
{
    public static class TinyUrlInterfaceExtensions
    {
        public static void CopyFieldsFrom(this TinyUrlInterface target, TinyUrlInterface source)
        {
            target.TinyUrl = source.TinyUrl;
        }
    }
    public interface TinyUrlInterface
    {
        string TinyUrl { get; set; }
    }

    public static class EntityWithTinyUrlInterfaceExtensions
    {
        public static void CopyFieldsFrom(this EntityWithTinyUrlInterface target, EntityWithTinyUrlInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            TinyUrlInterfaceExtensions.CopyFieldsFrom(target, source);
        }
    }

    public interface EntityWithTinyUrlInterface : EntityInterface, AggregateRootInterface, TinyUrlInterface
    {
        
    }

    public class EntityKeyWithTinyUrl : EntityWithTinyUrlInterface
    {
        public string TinyUrl { get; set; }
        public string Id { get; set; }
        public string FriendlyId { get; set; }
        public int Version { get; set; }
        public Type PrimaryInterface { get; set; }
    }
}