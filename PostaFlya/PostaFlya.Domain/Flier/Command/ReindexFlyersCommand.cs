using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Flier.Event;
using PostaFlya.Domain.Flier.Query;
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
            const int skiptake = 1000;

            IList<EntityIdInterface> flierIds = new List<EntityIdInterface>();
            do
            {
                var skip = flierIds.Any() ? _queryService.FindById<PostaFlya.Domain.Flier.Flier>(flierIds.Last().Id) : null;
                flierIds = _flierSearchService.IterateAllIndexedFliers(skiptake, skip);

                var fliers = _queryService.FindByIds<Flier>(flierIds.Select(i  => i.Id)).ToList();
                foreach (var flier in fliers)
                {
                    _domainEventPublishService.Publish(
                            new FlierModifiedEvent()
                            {
                                OrigState = flier,
                                NewState = flier
                            }
                    );
                }

            } while (flierIds.Any());
            return true;
        }
    }
}