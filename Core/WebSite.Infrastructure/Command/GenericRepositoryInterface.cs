using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Domain;

namespace WebSite.Infrastructure.Command
{
    public interface GenericRepositoryInterface : RepositoryInterface 
    {
        void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) where UpdateType : class, new();
        void UpdateEntity(Type entity, string id, Action<object> updateAction);
        void Store<EntityType>(EntityType entity);
    }
}
