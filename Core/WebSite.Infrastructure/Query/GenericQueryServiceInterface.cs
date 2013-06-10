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
        EntityRetType FindById<EntityRetType>(string id) 
            where EntityRetType : class, AggregateRootInterface, new();

        object FindById(Type entity, string id);

        EntityRetType FindByAggregate<EntityRetType>(string id, string aggregateRootId)
            where EntityRetType : class, AggregateInterface, new();
        
        object FindByAggregate(Type entity, string id, string aggregateRootId);
        

        IQueryable<string> FindAggregateEntityIds<EntityRetType>(string aggregateRootId)
            where EntityRetType : class, AggregateInterface, new();

        //common sense only use these for small data sets
        IQueryable<string> GetAllIds<EntityRetType>() where EntityRetType : class, AggregateRootInterface, new();
        IQueryable<string> GetAllIds(Type type);

    }

    public static class GenericQueryServiceInterfaceExtension
    {
        public static IQueryable<EntityRetType> FindAggregateEntities<EntityRetType>(this GenericQueryServiceInterface queryService, 
            string aggregateRootId)
            where EntityRetType : class, AggregateInterface, new()
        {
            return queryService.FindAggregateEntityIds<EntityRetType>(aggregateRootId)
                .Select(id => queryService.FindByAggregate<EntityRetType>(id, aggregateRootId));
        }

        public static IEnumerable<EntityRetType> FindByAggregateIds<EntityRetType>(this GenericQueryServiceInterface queryService
            , IEnumerable<AggregateInterface> ids )
            where EntityRetType : class, AggregateInterface, new()
        {
            return ids.Select(a => queryService.FindByAggregate<EntityRetType>(a.Id, a.AggregateId));
        }

//        public static IQueryable<EntityRetType> GetAll<EntityRetType>(this GenericQueryServiceInterface queryService)
//                where EntityRetType : class, AggregateInterface, new()
//        {
//            return queryService.GetAllIds<EntityRetType>()
//                .Select(id => queryService.FindById<EntityRetType>(id));
//        }
//
//        public static IQueryable<object> GetAll(this GenericQueryServiceInterface queryService, Type type)
//        {
//            return queryService.GetAllIds(type)
//                .Select(id => queryService.FindById(type, id));
//        }

        public static IEnumerable<EntityRetType> FindByIds<EntityRetType>(this GenericQueryServiceInterface queryService, IEnumerable<string> ids) 
            where EntityRetType : class, AggregateRootInterface, new()
        {
            return ids.Select(queryService.FindById<EntityRetType>);
        }

        public static IEnumerable<object> FindByIds(this GenericQueryServiceInterface queryService, Type type, IEnumerable<string> ids)
        {
            return ids.Select(id => queryService.FindById(type, id));
        }
    }
}
