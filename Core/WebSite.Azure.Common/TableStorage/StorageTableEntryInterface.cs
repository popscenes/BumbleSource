using System;

namespace WebSite.Azure.Common.TableStorage
{
    public static class StorageTableEntryInterfaceExtensions
    {
        public static EntityInterfaceType CreateEntityCopy<EntityType, EntityInterfaceType>(this StorageTableEntryInterface<EntityInterfaceType> ts)
            where EntityType : class, EntityInterfaceType, new()
        {
            var ret = new EntityType();
            ts.UpdateEntity(ret);
            return ret;
        }
    }

    public interface StorageTableEntryInterface
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
        int PartitionClone { get; set; }
        bool KeyChanged { get; set; }
    }

    public interface StorageTableEntryInterface<in EntityInterfaceType> : StorageTableEntryInterface
    {
        void Update(EntityInterfaceType source);
        void UpdateEntity(EntityInterfaceType target);
    }
}
