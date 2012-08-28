using System;

namespace Website.Azure.Common.TableStorage
{
    public static class StorageTableEntryInterfaceExtensions
    {
//        public static EntityInterfaceType CreateEntityCopy<EntityType, EntityInterfaceType>(this StorageTableEntryInterface<EntityInterfaceType> ts)
//            where EntityType : class, EntityInterfaceType, new()
//        {
//            var ret = new EntityType();
//            ts.UpdateEntity(ret);
//            return ret;
//        }
    }

    public interface StorageTableEntryInterface
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
        int PartitionClone { get; set; }
        bool KeyChanged { get; set; }
        void UpdateEntry(object source);
        object GetEntity(Type entityTyp);
        
    }

    public static class StorageTableEntryInterfaceExtension
    {
        public static EntityType GetEntity<EntityType>(this StorageTableEntryInterface storageTableEntry) where EntityType : class
        {
            return storageTableEntry.GetEntity(typeof (EntityType)) as EntityType;
        }

    }
}
