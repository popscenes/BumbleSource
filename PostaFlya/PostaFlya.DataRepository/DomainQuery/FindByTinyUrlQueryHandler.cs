using System.Linq;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Indexes;
using Website.Application.Domain.TinyUrl.Query;
using Website.Azure.Common.TableStorage;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery
{
    public class FindByTinyUrlQueryHandler : QueryHandlerInterface<FindByTinyUrlQuery, EntityWithTinyUrlInterface>
    {
        private readonly TableIndexServiceInterface _indexService;

        public FindByTinyUrlQueryHandler(TableIndexServiceInterface indexService)
        {
            _indexService = indexService;
        }

        public EntityWithTinyUrlInterface Query(FindByTinyUrlQuery argument)
        {
            var entries = _indexService.FindEntitiesByIndex<EntityWithTinyUrlInterface, JsonTableEntry>(new TinyUrlIndex<EntityWithTinyUrlInterface>(), 
                                                                                             argument.Url);
            var entry = entries.FirstOrDefault();
            if (entry == null)
                return null;

            return entry.GetEntity() as EntityWithTinyUrlInterface;

        }
    }
}