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
    public class GetAggregateByBrowserIdQueryHandler<EntityType> :
        QueryHandlerInterface<GetByBrowserIdQuery, List<EntityType>> where EntityType : class, AggregateInterface, BrowserIdInterface, new()
    {
        private readonly TableIndexServiceInterface _indexService;
        private readonly GenericQueryServiceInterface _queryService;

        public GetAggregateByBrowserIdQueryHandler(TableIndexServiceInterface indexService, GenericQueryServiceInterface queryService)
        {
            _indexService = indexService;
            _queryService = queryService;
        }

        public List<EntityType> Query(GetByBrowserIdQuery argument)
        {
            var entries = _indexService.FindEntitiesByIndex<EntityType, StorageTableKey>(DomainIndexSelectors.BrowserIdIndex,
                                                                                         argument.BrowserId);
            return _queryService.FindByAggregateIds<EntityType>(entries.Select(_ 
                                                                               => new AggregateIds()
                                                                                   {
                                                                                       AggregateId = StorageKeyUtil.ExtractAggregateRootEntityIdFromRowKey(_.RowKey),
                                                                                       Id = StorageKeyUtil.ExtractEntityIdFromRowKey(_.RowKey)
                                                                                   }))
                                .ToList();
        }
    }
}