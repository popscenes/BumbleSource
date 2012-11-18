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

        EntityRetType FindByFriendlyId<EntityRetType>(string id) where EntityRetType : class, new();
        object FindByFriendlyId(Type entity, string id);

        IQueryable<string> FindAggregateEntityIds<EntityRetType>(string aggregateRootId)
            where EntityRetType : class, AggregateInterface, new();
    }

    public static class GenericQueryServiceInterfaceExtension
    {
        public static IQueryable<EntityRetType> FindAggregateEntities<EntityRetType>(this GenericQueryServiceInterface queryService, 
            string aggregateRootId)
            where EntityRetType : class, AggregateInterface, new()
        {
            return queryService.FindAggregateEntityIds<EntityRetType>(aggregateRootId)
                .Select(id => queryService.FindById<EntityRetType>(id));
        }
    }
}
