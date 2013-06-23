using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Infrastructure.Domain;

namespace Website.Infrastructure.Command
{
    public interface GenericRepositoryInterface : RepositoryInterface 
    {
        void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) 
            where UpdateType : class,  AggregateRootInterface, new();
        void UpdateAggregateEntity<UpdateType>(string id, string aggregateRootId, Action<UpdateType> updateAction)
            where UpdateType : class,  AggregateInterface, new();
        
        void UpdateEntity(Type entityTyp, string id, Action<object> updateAction);
        void UpdateAggregateEntity(Type entityTyp, string id, string aggregateRootId, Action<object> updateAction);

        void Store<EntityType>(EntityType entity);
    }
}
