using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;

namespace PostaFlya.DataRepository.Search.Event
{
    public class EntityIndexEventHandler<EntityType> :
        HandleEventInterface<EntityModifiedEvent<EntityType>> where EntityType : class, EntityIdInterface
    {
        private readonly TableIndexServiceInterface _indexService;

        public EntityIndexEventHandler(TableIndexServiceInterface indexService)
        {
            _indexService = indexService;
        }

        public bool Handle(EntityModifiedEvent<EntityType> @event) 
        {
            var entity = @event.Entity;
            _indexService.UpdateEntityIndexes(entity, @event.Entity == null);
            return true;
        }

    }
}
