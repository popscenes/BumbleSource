using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Browser.Event;
using PostaFlya.Domain.Flier.Event;
using PostaFlya.Domain.Flier.Query;
using Website.Domain.Claims;
using Website.Domain.Claims.Event;
using Website.Domain.Content;
using Website.Domain.Content.Event;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Command
{
    [Serializable]
    public class ReindexFlyersCommand : DefaultCommandBase
    {
    }

    public class ReindexFlyersCommandHandler : CommandHandlerInterface<ReindexFlyersCommand>
    {
        private readonly FlierSearchServiceInterface _flierSearchService;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;
        private readonly GenericQueryServiceInterface _queryService;


        public ReindexFlyersCommandHandler(FlierSearchServiceInterface flierSearchService, 
            DomainEventPublishServiceInterface domainEventPublishService, GenericQueryServiceInterface queryService)
        {
            _flierSearchService = flierSearchService;
            _domainEventPublishService = domainEventPublishService;
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
                _domainEventPublishService.Publish(
                    new FlierModifiedEvent()
                        {
                            OrigState = flier,
                            NewState = flier
                        });

                var claims = _queryService.FindAggregateEntities<Claim>(flier.Id);
                foreach (var claim in claims)
                {
                    _domainEventPublishService.Publish(
                        new ClaimEvent()
                        {
                            OrigState = claim,
                            NewState = claim
                        });
                }
            }

            var boards = _queryService.FindByIds<Board>(_queryService.GetAllIds<Board>());
            foreach (var board in boards)
            {
                _domainEventPublishService.Publish(
                    new BoardModifiedEvent()
                    {
                        OrigState = board,
                        NewState = board
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
                _domainEventPublishService.Publish(
                    new BrowserModifiedEvent()
                    {
                        OrigState = browser,
                        NewState = browser
                    });
            }

            var images = _queryService.FindByIds<Image>(_queryService.GetAllIds<Image>());
            foreach (var image in images)
            {
                _domainEventPublishService.Publish(
                    new ImageModifiedEvent()
                    {
                        OrigState = image,
                        NewState = image
                    });
            }




//        , HandleEventInterface<ClaimEvent>

//        , HandleEventInterface<ImageModifiedEvent>
        }
    }
}