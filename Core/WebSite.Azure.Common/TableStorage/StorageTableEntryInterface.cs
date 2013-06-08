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

    public interface StorageTableEntryInterface : StorageTableKeyInterface
    {
        int PartitionClone { get; set; }
        bool KeyChanged { get; set; }
        void Init(object source);
        void UpdateEntry();
        object GetEntity(Type entityTyp = null);
        
    }


    public static class StorageTableEntryInterfaceExtension
    {
        public static EntityType GetEntity<EntityType>(this StorageTableEntryInterface storageTableEntry) where EntityType : class
        {
            return storageTableEntry.GetEntity(typeof (EntityType)) as EntityType;
        }

    }
}
