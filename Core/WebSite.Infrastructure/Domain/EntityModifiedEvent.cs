using System;
using System.Reflection;

namespace Website.Infrastructure.Domain
{
    /// <summary>
    /// Null OrigState means a new entity
    /// Null NewState means a deleted entity
    /// </summary>
    /// <typeparam name="EntityType">EnityType that has been modified</typeparam>
    
    [Serializable]
    public sealed class EntityModifiedEvent<EntityType> : EventBase, EntityModifiedEventInterface<EntityType>
    {
        public EntityModifiedEvent()
        {
        }


        public EntityModifiedEvent(bool isDeleted)
        {
            IsDeleted = isDeleted;
        }



        public EntityType Entity { get; set; }
        public bool IsDeleted { get; set; }
    }

    public static class EntityModifiedEventCreator
    {
        private static readonly Type Container;
        static EntityModifiedEventCreator()
        {
            Container = typeof(EntityModifiedEvent<>);
        }

        public static EventInterface CreateFor(object source, bool isdeleted = false)
        {
            var type = Container.MakeGenericType(source.GetType());
            var prop = type.GetProperty("Entity");
            var meth = prop.GetSetMethod();

            var act = Activator.CreateInstance(type, (object)isdeleted);
            meth.Invoke(act, new[]{ source });
            return act as EventInterface;
        }
    }
}