using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Behaviour.Command
{
    public interface FlierBehaviourDefaultRespositoryInterface : GenericRepositoryInterface
        , GenericQueryServiceInterface
    {
        
    }
    public class FlierBehaviourDefaultRespository : FlierBehaviourDefaultRespositoryInterface
    {

        public void Store(object entity)
        {
            Store(entity as FlierBehaviourInterface);
        }

        public bool SaveChanges()
        {
            return true;
        }


        public void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) where UpdateType : class, AggregateRootInterface, new()
        {

        }

        public void UpdateAggregateEntity<UpdateType>(string id, string aggregateRootId, Action<UpdateType> updateAction) where UpdateType : class, AggregateInterface, new()
        {
            
        }

        public void UpdateEntity(Type entityTyp, string id, Action<object> updateAction)
        {
            
        }

        public void UpdateAggregateEntity(Type entityTyp, string id, string aggregateRootId, Action<object> updateAction)
        {

        }

        public void Store<EntityType>(EntityType entity)
        {

        }

        public EntityRetType FindById<EntityRetType>(string id) where EntityRetType : class, AggregateRootInterface, new()
        {
            return null;
        }

        public object FindById(Type entity, string id)
        {
            return null;
        }

        public EntityRetType FindByAggregate<EntityRetType>(string id, string aggregateRootId) where EntityRetType : class, AggregateInterface, new()
        {
            return null;
        }

        public object FindByAggregate(Type entity, string id, string aggregateRootId)
        {
            return null;
        }


        public IQueryable<string> FindAggregateEntityIds<EntityRetType>(string aggregateRootId)
            where EntityRetType : class, AggregateInterface, new()
        {
            return new List<string>().AsQueryable();
        }

        public IQueryable<string> GetAllIds<EntityRetType>() where EntityRetType : class, AggregateRootInterface, new()
        {
            return null;
        }

        public IQueryable<string> GetAllIds(Type type)
        {
            return null;
        }

        public IQueryable<AggregateInterface> GetAllAggregateIds<EntityRetType>() where EntityRetType : class, AggregateInterface, new()
        {
            return null;
        }
    }
}
