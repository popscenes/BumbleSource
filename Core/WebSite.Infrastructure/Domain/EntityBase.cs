using System;

namespace Website.Infrastructure.Domain
{
    [Serializable]
    public class EntityBase<EntityInterfaceType> where EntityInterfaceType : class, EntityInterface
    {
        public string Id { get; set; }
        public string FriendlyId { get; set; }

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