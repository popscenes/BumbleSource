using System.Collections.Generic;
using System.Linq;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Indexes;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Browser
{
    public class GetByBrowserIdQueryHandler<EntityType> :
        QueryHandlerInterface<GetByBrowserIdQuery<EntityType>, List<EntityType>> where EntityType : class, AggregateRootInterface, BrowserIdInterface, new()
    {
        private readonly TableIndexServiceInterface _indexService;
        private readonly GenericQueryServiceInterface _queryService;

        public GetByBrowserIdQueryHandler(TableIndexServiceInterface indexService, GenericQueryServiceInterface queryService)
        {
            _indexService = indexService;
            _queryService = queryService;
        }

        public List<EntityType> Query(GetByBrowserIdQuery<EntityType> argument)
        {
            var entries = _indexService.FindEntitiesByIndex<EntityType, StorageTableKey>(new BrowserIdIndex<EntityType>(), 
                                                                                         argument.BrowserId);
            return _queryService.FindByIds<EntityType>(entries.Select(_ => _.RowKey.ExtractEntityIdFromRowKey()))
                                .ToList();
        }
    }
}