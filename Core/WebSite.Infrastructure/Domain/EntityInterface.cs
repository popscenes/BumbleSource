using System;

namespace Website.Infrastructure.Domain
{
    public static class EntityInterfaceExtensions
    {
        public static void CopyFieldsFrom(this EntityInterface target, EntityInterface source)
        {
            EntityIdInterfaceExtensions.CopyFieldsFrom(target, source);    
            target.Version = source.Version;
        }

        public static EntityInterfaceType CreateCopy<EntityType, EntityInterfaceType>(this EntityInterfaceType source, Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where EntityType : class, EntityInterfaceType, new()
        {
            var newb = new EntityType();
            copyFields(newb, source);
            return newb;
        }
    }
     
    public static class EntityIdInterfaceExtensions
    {
        public static void CopyFieldsFrom(this EntityIdInterface target, EntityIdInterface source)
        {
            target.Id = source.Id;
            target.FriendlyId = source.FriendlyId;
        }
    }

    public interface EntityInterface : EntityIdInterface
    {
        int Version { get; set; }
        Type PrimaryInterface { get; }
    }

    public interface EntityIdInterface
    {
        string Id { get; set; }
        string FriendlyId { get; set; }
    }

    public static class AggregateInterfaceExtensions
    {
        public static void CopyFieldsFrom(this AggregateInterface target, AggregateInterface source)
        {
            target.AggregateId = source.AggregateId;
            target.AggregateTypeTag = source.AggregateTypeTag;
        }
    }

    public interface AggregateInterface
    {
        string AggregateId { get; set; }
        string AggregateTypeTag { get; set; } 
    }
}