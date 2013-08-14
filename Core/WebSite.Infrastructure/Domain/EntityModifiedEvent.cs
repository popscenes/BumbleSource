using System;

namespace Website.Infrastructure.Domain
{
    /// <summary>
    /// Null OrigState means a new entity
    /// Null NewState means a deleted entity
    /// </summary>
    /// <typeparam name="EntityType">EnityType that has been modified</typeparam>
    
    [Serializable]
    public class EntityModifiedEvent<EntityType> : EventBase, EntityModifiedEventInterface<EntityType>
        where EntityType : class, EntityInterface
    {
        public EntityType OrigState { get; set; }
        public EntityType NewState { get; set; }
    }
}