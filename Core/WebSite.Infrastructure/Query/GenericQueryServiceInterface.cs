using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Infrastructure.Query
{

    public interface GenericQueryServiceInterface : QueryServiceInterface
    {
        EntityRetType FindById<EntityRetType>(string id) where EntityRetType : class, new();
        object FindById(Type entity, string id);
        IQueryable<EntityRetType> FindAggregateEntities<EntityRetType>(string aggregateRootId, int take = -1)
            where EntityRetType : class, AggregateInterface, new();
    }
}
