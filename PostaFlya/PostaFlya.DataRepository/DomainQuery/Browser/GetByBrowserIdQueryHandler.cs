using System.Collections.Generic;
using System.Linq;
using PostaFlya.DataRepository.Binding;
using Website.Application.Domain.Browser.Query;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Browser
{
    public class GetByBrowserIdQueryHandler<EntityType> :
        QueryHandlerInterface<GetByBrowserIdQuery, List<EntityType>> where EntityType : class, AggregateRootInterface, BrowserIdInterface, new()
    {
        private readonly TableIndexServiceInterface _indexService;
        private readonly GenericQueryServiceInterface _queryService;

        public GetByBrowserIdQueryHandler(TableIndexServiceInterface indexService, GenericQueryServiceInterface queryService)
        {
            _indexService = indexService;
            _queryService = queryService;
        }

        public List<EntityType> Query(GetByBrowserIdQuery argument)
        {
            var entries = _indexService.FindEntitiesByIndex<EntityType, StorageTableKey>(DomainIndexSelectors.BrowserIdIndex,
                                                                                         argument.BrowserId);
            return _queryService.FindByIds<EntityType>(entries.Select(_ => _.RowKey.ExtractEntityIdFromRowKey()))
                                .ToList();
        }
    }
}