using System.Collections.Generic;
using PostaFlya.Domain.Flier.Event;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Service;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier.Command
{
    internal class EditFlierCommandHandler : CommandHandlerInterface<EditFlierCommand>
    {
        private readonly FlierRepositoryInterface _flierRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly FlierQueryServiceInterface _flierQueryService;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;


        public EditFlierCommandHandler(FlierRepositoryInterface flierRepository
            ,UnitOfWorkFactoryInterface unitOfWorkFactory
            , FlierQueryServiceInterface flierQueryService, DomainEventPublishServiceInterface domainEventPublishService)
        {
            _flierRepository = flierRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _flierQueryService = flierQueryService;
            _domainEventPublishService = domainEventPublishService;
        }

        public object Handle(EditFlierCommand command)
        {
            var flierQuery = _flierQueryService.FindById<Flier>(command.Id);
            if (flierQuery == null || flierQuery.BrowserId == null || !flierQuery.BrowserId.Equals(command.BrowserId))
                return false;

            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _flierRepository }))
            {
                _flierRepository.UpdateEntity<Flier>(command.Id, 
                    flier =>
                        {
                            flier.FriendlyId = _flierQueryService.FindFreeFriendlyId(flierQuery);
                            flier.Title = command.Title;
                            flier.Description = command.Description;
                            flier.Tags = command.Tags;
                            flier.Location = command.Location;
                            flier.Image = command.Image;
                            flier.EffectiveDate = command.EffectiveDate;
                            flier.ImageList = command.ImageList;
                        });                          
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Flier Edit Failed", true)
                    .AddCommandId(command);

            _domainEventPublishService.Publish(
                new FlierModifiedEvent()
                    {
                        OrigState = flierQuery,
                        NewState = _flierQueryService.FindById<Flier>(command.Id)
                    }
                );

            return new MsgResponse("Flier Edit", false)
                .AddEntityId(command.Id)
                .AddCommandId(command);
        }
    }
}