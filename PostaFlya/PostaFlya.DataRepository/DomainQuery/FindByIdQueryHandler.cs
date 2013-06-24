using System.Collections.Generic;
using System.Linq;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery
{
    public class FindByIdQueryHandler<EntityType> :
        QueryHandlerInterface<FindByIdQuery, EntityType> where EntityType : class, AggregateRootInterface, new()
    {
        private readonly GenericQueryServiceInterface _queryService;


        public FindByIdQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public EntityType Query(FindByIdQuery argument)
        {
            return _queryService.FindById<EntityType>(argument.Id);
        }
    }

    public class FindByIdsQueryHandler<EntityType> :
    QueryHandlerInterface<FindByIdsQuery, List<EntityType>> where EntityType : class, AggregateRootInterface, new()
    {
        private readonly GenericQueryServiceInterface _queryService;


        public FindByIdsQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public List<EntityType> Query(FindByIdsQuery argument)
        {
            return _queryService.FindByIds<EntityType>(argument.Ids).ToList();
        }
    }
}