using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Browser.Event;
using PostaFlya.Domain.Flier.Event;
using Website.Azure.Common.TableStorage;
using Website.Domain.Claims.Event;
using Website.Domain.Comments.Event;
using Website.Domain.Content.Event;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Publish;

namespace PostaFlya.DataRepository.Search.Event
{
    public class EntityIndexEventHandler :
        HandleEventInterface<FlierModifiedEvent>
        , HandleEventInterface<BoardModifiedEvent>
        , HandleEventInterface<ClaimEvent>
        , HandleEventInterface<CommentEvent>
        , HandleEventInterface<BrowserModifiedEvent>
        , HandleEventInterface<ImageModifiedEvent>
    {
        private readonly TableIndexServiceInterface _indexService;

        public EntityIndexEventHandler(TableIndexServiceInterface indexService)
        {
            _indexService = indexService;
        }

        protected bool HandleInternal<EntityType>(EntityModifiedDomainEvent<EntityType> @event) 
            where EntityType : class, EntityInterface
        {
            var entity = @event.NewState ?? @event.OrigState;
            _indexService.UpdateEntityIndexes(entity, @event.NewState == null);
            return true;
        }

        public bool Handle(FlierModifiedEvent @event)
        {
            return HandleInternal(@event);
        }

        public bool Handle(BoardModifiedEvent @event)
        {
            return HandleInternal(@event);
        }

        public bool Handle(ClaimEvent @event)
        {
            return HandleInternal(@event);
        }

        public bool Handle(CommentEvent @event)
        {
            return HandleInternal(@event);
        }

        public bool Handle(BrowserModifiedEvent @event)
        {
            return HandleInternal(@event);
        }

        public bool Handle(ImageModifiedEvent @event)
        {
            return HandleInternal(@event);
        }
    }
}
