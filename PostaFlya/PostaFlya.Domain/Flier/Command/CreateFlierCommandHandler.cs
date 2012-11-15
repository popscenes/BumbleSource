using System;
using System.Collections.Generic;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Flier.Event;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Service;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Query;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using System.Linq;

namespace PostaFlya.Domain.Flier.Command
{
    internal class CreateFlierCommandHandler : CommandHandlerInterface<CreateFlierCommand>
    {
        private readonly FlierRepositoryInterface _flierRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly FlierQueryServiceInterface _flierQueryService;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;
        private readonly CommandBusInterface _commandBus;
        private readonly BrowserQueryServiceInterface _browserQueryService;

        public CreateFlierCommandHandler(FlierRepositoryInterface flierRepository
            , UnitOfWorkFactoryInterface unitOfWorkFactory, FlierQueryServiceInterface flierQueryService
            , DomainEventPublishServiceInterface domainEventPublishService
            , CommandBusInterface commandBus
            , BrowserQueryServiceInterface browserQueryService)
        {
            _flierRepository = flierRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _flierQueryService = flierQueryService;
            _domainEventPublishService = domainEventPublishService;
            _commandBus = commandBus;
            _browserQueryService = browserQueryService;
        }

        public object Handle(CreateFlierCommand command)
        {
            var newFlier = new Flier(command.Location)
                               {
                                   BrowserId = command.BrowserId,
                                   Title = command.Title,
                                   Description = command.Description,
                                   Tags = command.Tags,
                                   Image = command.Image,
                                   CreateDate = DateTime.UtcNow,
                                   FlierBehaviour = command.FlierBehaviour,
                                   EffectiveDate = command.EffectiveDate == default(System.DateTime) ? DateTime.UtcNow : command.EffectiveDate,
                                   ImageList = command.ImageList,
                                   ExternalSource = command.ExternalSource,
                                   ExternalId = command.ExternalId
                               };

            if(newFlier.FlierBehaviour == FlierBehaviour.Default)
                newFlier.Status = FlierStatus.Active;

            if (command.AttachTearOffs)
            {
                newFlier.Features.Add(new SimpleEntityFeature() { FeatureType = FeatureType.TearOff, Cost = 2.00, BrowserId = command.BrowserId });
            }

            newFlier.FriendlyId = _flierQueryService.FindFreeFriendlyId(newFlier);

            List<BoardFlierModifiedEvent> boardFliers = null;
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _flierRepository }))
            {      
                _flierRepository.Store(newFlier);
                boardFliers = AddFlierToBoardCommandHandler.UpdateAddFlierToBoards(command.BoardSet, newFlier, _flierQueryService,
                                                                     _flierRepository);
            }

            if(!unitOfWork.Successful)
                return new MsgResponse("Flier Creation Failed", true)
                        .AddCommandId(command);
            

            _domainEventPublishService.Publish(new FlierModifiedEvent() { NewState = newFlier });

            foreach (var boardFlierModifiedEvent in boardFliers)
            {
                _domainEventPublishService.Publish(boardFlierModifiedEvent);
            }

            return new MsgResponse("Flier Create", false)
                .AddEntityId(newFlier.Id)
                .AddCommandId(command);
        }
    }
}