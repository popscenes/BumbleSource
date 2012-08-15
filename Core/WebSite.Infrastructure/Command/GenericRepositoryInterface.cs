using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Domain;

namespace WebSite.Infrastructure.Command
{
    public interface GenericRepositoryInterface<EntityType> : RepositoryInterface 
    {
        void UpdateEntity(string id, Action<EntityType> updateAction);
        void Store(EntityType entity);
    }
}
