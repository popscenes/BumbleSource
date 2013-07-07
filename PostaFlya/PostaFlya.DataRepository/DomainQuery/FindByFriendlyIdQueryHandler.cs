using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery
{
    public class FindByFriendlyIdQueryHandler<EntityType> :
        QueryHandlerInterface<FindByFriendlyIdQuery<EntityType>, EntityType> where EntityType : class, AggregateRootInterface, new()
    {
        private readonly TableIndexServiceInterface _indexService;
        private readonly GenericQueryServiceInterface _queryService;


        public FindByFriendlyIdQueryHandler(TableIndexServiceInterface indexService, GenericQueryServiceInterface queryService)
        {
            _indexService = indexService;
            _queryService = queryService;
        }

        public EntityType Query(FindByFriendlyIdQuery<EntityType> argument)
        {
            var entries = _indexService.FindEntitiesByIndex<EntityType, StorageTableKey>(StandardIndexSelectors.FriendlyIdIndex,
                                                               argument.FriendlyId);
            return !entries.Any() ? default(EntityType) : _queryService.FindById<EntityType>(entries.First().RowKey.ExtractEntityIdFromRowKey());
        }
    }
}
