using System.Linq;
using PostaFlya.DataRepository.Binding;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Browser
{
    public class FindBrowserByIdentityProviderQueryHandler<BrowserType>
        : QueryHandlerInterface<FindBrowserByIdentityProviderQuery, BrowserType>
        where BrowserType : Website.Domain.Browser.Browser, new()
    {
        private readonly TableIndexServiceInterface _indexService;
        private readonly GenericQueryServiceInterface _queryService;
        public FindBrowserByIdentityProviderQueryHandler(TableIndexServiceInterface indexService, GenericQueryServiceInterface queryService)
        {
            _indexService = indexService;
            _queryService = queryService;
        }

        public BrowserType Query(FindBrowserByIdentityProviderQuery argument)
        {
            var rec = _indexService.FindEntitiesByIndex<Domain.Browser.Browser, StorageTableKey>(
                DomainIndexSelectors.BrowserCredentialIndex
                , argument.Credential.ToUniqueString()).SingleOrDefault();

            if (rec == null)
                return null;

            return _queryService.FindById<BrowserType>(rec.RowKey.ExtractEntityIdFromRowKey());
        }
    }
}