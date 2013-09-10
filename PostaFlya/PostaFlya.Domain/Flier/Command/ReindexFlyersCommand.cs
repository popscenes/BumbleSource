using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Boards;
using Website.Domain.Claims;
using Website.Domain.Content;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Command
{
    [Serializable]
    public class ReindexFlyersCommand : DefaultCommandBase
    {
    }

    public class ReindexFlyersCommandHandler : MessageHandlerInterface<ReindexFlyersCommand>
    {
        private readonly EventPublishServiceInterface _eventPublishService;
        private readonly GenericQueryServiceInterface _queryService;


        public ReindexFlyersCommandHandler(EventPublishServiceInterface eventPublishService, GenericQueryServiceInterface queryService)
        {
            _eventPublishService = eventPublishService;
            _queryService = queryService;
        }

        public object Handle(ReindexFlyersCommand command)
        {
            ReindexAll();
            return true;
        }

        private void ReindexAll()
        {
            var fliers = _queryService.FindByIds<Flier>(_queryService.GetAllIds<Flier>());
            foreach (var flier in fliers)
            {
                _eventPublishService.Publish(
                    new EntityModifiedEvent<Flier>()
                        {
                            Entity = flier
                        });

                var claims = _queryService.FindAggregateEntities<Claim>(flier.Id);
                foreach (var claim in claims)
                {
                    _eventPublishService.Publish(
                        new EntityModifiedEvent<Claim>()
                        {
                            Entity = claim
                        });
                }
            }

            var boards = _queryService.FindByIds<Board>(_queryService.GetAllIds<Board>());
            foreach (var board in boards)
            {
                _eventPublishService.Publish(
                    new EntityModifiedEvent<Board>()
                    {
                        Entity = board
                    });
//                var boardfliers = _queryService.FindAggregateEntities<BoardFlier>(board.Id);
//                foreach (var boardflier in boardfliers)
//                {
//                    _domainEventPublishService.Publish(
//                        new BoardFlierModifiedEvent()
//                        {
//                            OrigState = boardflier,
//                            NewState = boardflier
//                        });
//                }
            }

            var browsers = _queryService.FindByIds<Browser.Browser>(_queryService.GetAllIds<Browser.Browser>());
            foreach (var browser in browsers)
            {
                _eventPublishService.Publish(
                    new EntityModifiedEvent<Browser.Browser>()
                    {
                        Entity = browser
                    });
            }

            var images = _queryService.FindByIds<Image>(_queryService.GetAllIds<Image>());
            foreach (var image in images)
            {
                _eventPublishService.Publish(
                    new EntityModifiedEvent<Image>()
                    {
                        Entity = image
                    });
            }




//        , HandleEventInterface<ClaimEvent>

//        , HandleEventInterface<ImageModifiedEvent>
        }
    }
}