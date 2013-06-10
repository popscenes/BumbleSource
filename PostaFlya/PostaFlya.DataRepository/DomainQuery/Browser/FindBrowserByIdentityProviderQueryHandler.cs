using System.Linq;
using PostaFlya.DataRepository.Binding;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Browser
{
    public class FindBrowserByIdentityProviderQueryHandler 
        : QueryHandlerInterface<FindBrowserByIdentityProviderQuery, Domain.Browser.Browser>
    {
        private readonly TableIndexServiceInterface _indexService;
        private readonly GenericQueryServiceInterface _queryService;
        public FindBrowserByIdentityProviderQueryHandler(TableIndexServiceInterface indexService, GenericQueryServiceInterface queryService)
        {
            _indexService = indexService;
            _queryService = queryService;
        }

        public Domain.Browser.Browser Query(FindBrowserByIdentityProviderQuery argument)
        {
            var rec = _indexService.FindEntitiesByIndex<Domain.Browser.Browser, StorageTableKey>(
                DomainIndexSelectors.BrowserCredentialIndex
                , argument.Credential.ToUniqueString()).SingleOrDefault();

            return _queryService.FindById<Domain.Browser.Browser>(rec.RowKey.ExtractEntityIdFromRowKey());
        }
    }
}