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

        //common sense only use these for small data sets
        IQueryable<string> GetAllIds<EntityRetType>() where EntityRetType : class, new();
        IQueryable<string> GetAllIds(Type type);

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

        public static IQueryable<EntityRetType> GetAll<EntityRetType>(this GenericQueryServiceInterface queryService)
                where EntityRetType : class, AggregateInterface, new()
        {
            return queryService.GetAllIds<EntityRetType>()
                .Select(id => queryService.FindById<EntityRetType>(id));
        }

        public static IQueryable<object> GetAll(this GenericQueryServiceInterface queryService, Type type)
        {
            return queryService.GetAllIds(type)
                .Select(id => queryService.FindById(type, id));
        }
    }
}
