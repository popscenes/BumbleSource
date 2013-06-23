using System.Linq;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Query;

namespace Website.Mocks.Domain.DomainQuery.Browser
{
    public class TestFindBrowserByIdentityProviderQueryHandler<BrowserType>
        : QueryHandlerInterface<FindBrowserByIdentityProviderQuery, BrowserType>
        where BrowserType : Website.Domain.Browser.Browser, new()
    {
        private readonly GenericQueryServiceInterface _queryService;
        public TestFindBrowserByIdentityProviderQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public BrowserType Query(FindBrowserByIdentityProviderQuery argument)
        {
            var all = _queryService.GetAllIds<BrowserType>().Select(_queryService.FindById<BrowserType>);
            return all.FirstOrDefault(a => a.ExternalCredentials.Any(e => e.ToUniqueString() == argument.Credential.ToUniqueString()));  
        }
    }
}