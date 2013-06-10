using System.Linq;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Specification.DomainQuery
{
    public class TestFindByFriendlyIdQueryHandler<EntityType> : 
        QueryHandlerInterface<FindByFriendlyIdQuery, EntityType> where EntityType : class, AggregateRootInterface, new()
    {
        private readonly GenericQueryServiceInterface _queryService;


        public TestFindByFriendlyIdQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public EntityType Query(FindByFriendlyIdQuery argument)
        {
            var all = _queryService.GetAllIds<EntityType>().Select(_queryService.FindById<EntityType>);
            return all.FirstOrDefault(a => a.FriendlyId == argument.FriendlyId);    
        }
    }
}
