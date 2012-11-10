namespace Website.Infrastructure.Domain
{
    /// <summary>
    /// Null OrigState means a new entity
    /// Null NewState means a deleted entity
    /// </summary>
    /// <typeparam name="EntityType">EnityType that has been modified</typeparam>
    public class EntityModifiedDomainEvent<EntityType> : DomainEventBase, EntityModifiedDomainEventInterface<EntityType>
        where EntityType : EntityInterface
    {
        public EntityType OrigState { get; set; }
        public EntityType NewState { get; set; }
    }
}