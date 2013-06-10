using System;
using System.Collections.Generic;
using System.Linq;

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


        private static readonly char[] InvalidChars = new char[] {'/','\\','#','?'};
        public static void SanitizeEntityId(this EntityIdInterface target)
        {
            if (target == null || string.IsNullOrWhiteSpace(target.Id))
                return;
            target.Id = new string(target.Id.Where(c => !InvalidChars.Contains(c)).ToArray());
        }
    }

    public interface EntityInterface : EntityIdInterface
    {
        int Version { get; set; }
        Type PrimaryInterface { get; }
    }

    public class EntityIdEquals : IEqualityComparer<EntityIdInterface>
    {
        public bool Equals(EntityIdInterface x, EntityIdInterface y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(EntityIdInterface obj)
        {
            return obj.Id.GetHashCode();
        }
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
            EntityIdInterfaceExtensions.CopyFieldsFrom(target, source);
            target.AggregateId = source.AggregateId;
            target.AggregateTypeTag = source.AggregateTypeTag;
        }
    }

    public interface AggregateRootInterface : EntityIdInterface
    {

    }

    public interface AggregateInterface : EntityIdInterface
    {
        string AggregateId { get; set; }
        string AggregateTypeTag { get; set; } 
    }

    public class AggregateIds : AggregateInterface
    {
        public string Id { get; set; }
        public string FriendlyId { get; set; }
        public string AggregateId { get; set; }
        public string AggregateTypeTag { get; set; }
    }
}