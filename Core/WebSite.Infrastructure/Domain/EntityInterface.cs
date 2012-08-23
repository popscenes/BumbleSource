using System;

namespace WebSite.Infrastructure.Domain
{
    public static class EntityInterfaceExtensions
    {
        public static void CopyFieldsFrom(this EntityInterface target, EntityInterface source)
        {
            target.Id = source.Id;
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
    public interface EntityInterface : EntityIdInterface
    {
        int Version { get; set; }
        Type PrimaryInterface { get; }
    }

    public interface EntityIdInterface
    {
        string Id { get; set; }
    }

    public interface AggregateInterface
    {
        string AggregateId { get; set; }
    }
}