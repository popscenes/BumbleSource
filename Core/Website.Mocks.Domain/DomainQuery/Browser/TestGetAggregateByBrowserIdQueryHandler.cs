using System.Collections.Generic;
using System.Linq;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Mocks.Domain.DomainQuery.Browser
{
    public class TestGetAggregateByBrowserIdQueryHandler<EntityType> :
        QueryHandlerInterface<GetByBrowserIdQuery<EntityType>, List<EntityType>> where EntityType : class, AggregateInterface, BrowserIdInterface, new()
    {
        private readonly GenericQueryServiceInterface _queryService;

        public TestGetAggregateByBrowserIdQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public List<EntityType> Query(GetByBrowserIdQuery<EntityType> argument)
        {
            var all = _queryService.GetAllAggregateIds<EntityType>()
                .Select(a => _queryService.FindByAggregate<EntityType>(a.Id, a.AggregateId));
            return all.Where(a => a.BrowserId == argument.BrowserId).ToList();
        }
    }
}