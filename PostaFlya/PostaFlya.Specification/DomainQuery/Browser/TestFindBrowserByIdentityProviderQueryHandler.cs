using System.Linq;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Specification.DomainQuery.Browser
{
    public class TestFindBrowserByIdentityProviderQueryHandler 
        : QueryHandlerInterface<FindBrowserByIdentityProviderQuery, Domain.Browser.Browser>
    {
        private readonly GenericQueryServiceInterface _queryService;
        public TestFindBrowserByIdentityProviderQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public Domain.Browser.Browser Query(FindBrowserByIdentityProviderQuery argument)
        {
            var all = _queryService.GetAllIds<Domain.Browser.Browser>().Select(_queryService.FindById<Domain.Browser.Browser>);
            return all.FirstOrDefault(a => a.ExternalCredentials.Any(e => e.ToUniqueString() == argument.Credential.ToUniqueString()));  
        }
    }
}