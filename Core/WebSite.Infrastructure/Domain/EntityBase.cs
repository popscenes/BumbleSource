using System;

namespace WebSite.Infrastructure.Domain
{
    [Serializable]
    public abstract class EntityBase<EntityInterfaceType> where EntityInterfaceType : class, EntityInterface
    {
        public string Id { get; set; }

        public int Version { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as EntityInterfaceType);
        }

        public bool Equals(EntityInterfaceType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.Id == null && Id == null) return true;
            if (other.Id == null) return false;
            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            if (Id == null)
                return "".GetHashCode();

            return Id.GetHashCode();
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

    }
}