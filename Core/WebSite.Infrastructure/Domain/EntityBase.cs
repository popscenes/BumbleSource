using System;

namespace WebSite.Infrastructure.Domain
{
    [Serializable]
    public class EntityBase<EntityInterfaceType> where EntityInterfaceType : class, EntityInterface
    {
        public string Id { get; set; }

        public int Version { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var ent = obj as EntityBase<EntityInterfaceType>;
            if (ent == null) return false;
            return Equals(ent);
        }

        public Type PrimaryInterface
        {
            get { return typeof (EntityInterfaceType); }
        }


        public PropertyGroupCollection ExtendedProperties { get; set; }
        public object this[string group, string key]
        {
            get { return ExtendedProperties[group, key]; }
            set { ExtendedProperties[group, key] = value; }
        }

        public bool Equals(EntityBase<EntityInterfaceType> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Id, Id);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}