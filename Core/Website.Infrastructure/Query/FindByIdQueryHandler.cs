using System.Collections.Generic;
using System.Linq;
using Website.Infrastructure.Domain;

namespace Website.Infrastructure.Query
{
    public class FindByAggregateIdQueryHandler<EntityType> :
        QueryHandlerInterface<FindByAggregateIdQuery<EntityType>, List<EntityType>> 
        where EntityType : class, AggregateInterface, new()
    {
        private readonly GenericQueryServiceInterface _queryService;

        public List<EntityType> Query(FindByAggregateIdQuery<EntityType> argument)
        {
            return _queryService.FindAggregateEntities<EntityType>(argument.Id).ToList();
        }
    }

    public class FindByIdQueryHandler<EntityType> :
        QueryHandlerInterface<FindByIdQuery<EntityType>, EntityType> where EntityType : class, AggregateRootInterface, new()
    {
        private readonly GenericQueryServiceInterface _queryService;


        public FindByIdQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public EntityType Query(FindByIdQuery<EntityType> argument)
        {
            return _queryService.FindById<EntityType>(argument.Id);
        }
    }

    public class FindByIdsQueryHandler<EntityType> :
    QueryHandlerInterface<FindByIdsQuery<EntityType>, List<EntityType>> where EntityType : class, AggregateRootInterface, new()
    {
        private readonly GenericQueryServiceInterface _queryService;


        public FindByIdsQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public List<EntityType> Query(FindByIdsQuery<EntityType> argument)
        {
            return _queryService.FindByIds<EntityType>(argument.Ids).ToList();
        }
    }
}