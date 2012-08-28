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


        public void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) where UpdateType : class, new()
        {

        }

        public void UpdateEntity(Type entity, string id, Action<object> updateAction)
        {
            
        }

        public void Store<EntityType>(EntityType entity)
        {

        }

        public EntityRetType FindById<EntityRetType>(string id) where EntityRetType : class, new()
        {
            return null;
        }

        public object FindById(Type entity, string id)
        {
            return null;
        }

        public IQueryable<EntityRetType> FindAggregateEntities<EntityRetType>(string aggregateRootId, int take = -1) where EntityRetType : class, AggregateInterface, new()
        {
            return new List<EntityRetType>().AsQueryable();
        }
    }
}
