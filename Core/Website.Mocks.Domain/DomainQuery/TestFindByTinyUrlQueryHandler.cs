using System.Linq;
using Website.Application.Domain.TinyUrl.Query;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Query;

namespace Website.Mocks.Domain.DomainQuery
{
    public class TestFindByTinyUrlQueryHandler : QueryHandlerInterface<FindByTinyUrlQuery, EntityWithTinyUrlInterface>
    {
        private readonly GenericQueryServiceInterface _queryService;

        public TestFindByTinyUrlQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public EntityWithTinyUrlInterface Query(FindByTinyUrlQuery argument)
        {
            var all = _queryService.GetAllIds<EntityKeyWithTinyUrl>().Select(_queryService.FindById<EntityKeyWithTinyUrl>);
            return all.FirstOrDefault(a => a.TinyUrl == argument.Url);    

        }
    }
}