using System.Collections.Generic;
using System.Linq;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Mocks.Domain.DomainQuery.Browser
{
    public class TestGetByBrowserIdQueryHandler<EntityType> :
        QueryHandlerInterface<GetByBrowserIdQuery<EntityType>, List<EntityType>> where EntityType : class, AggregateRootInterface, BrowserIdInterface, new()
    {
        private readonly GenericQueryServiceInterface _queryService;

        public TestGetByBrowserIdQueryHandler( GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public List<EntityType> Query(GetByBrowserIdQuery<EntityType> argument)
        {
            var all = _queryService.GetAllIds<EntityType>().Select(_queryService.FindById<EntityType>);
            return all.Where(a => a.BrowserId == argument.BrowserId).ToList();        
        }
    }
}