using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Flier.Event;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Service;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using System.Linq;

namespace PostaFlya.Domain.Flier.Command
{
    internal class EditFlierCommandHandler : CommandHandlerInterface<EditFlierCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;


        public EditFlierCommandHandler(GenericRepositoryInterface repository
            ,UnitOfWorkFactoryInterface unitOfWorkFactory
            , GenericQueryServiceInterface queryService, DomainEventPublishServiceInterface domainEventPublishService)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _queryService = queryService;
            _domainEventPublishService = domainEventPublishService;
        }

        public object Handle(EditFlierCommand command)
        {
            var flierQuery = _queryService.FindById<Flier>(command.Id);
            if (flierQuery == null || flierQuery.BrowserId == null || !flierQuery.BrowserId.Equals(command.BrowserId))
                return false;

            List<BoardFlierModifiedEvent> boardFliers = null;
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {
                //
                var attachContactDetails = command.UseBrowserContactDetails && command.AttachContactDetails;

                if(!flierQuery.PaymentOptions.Any(_ => _.Type ==PaymentOptionType.ContactDetails && _.Status == PaymentOptionStatus.PaymentAccepted) )
                {
                    if (attachContactDetails)
                    {
                        flierQuery.PaymentOptions.Add(new PaymentOption{Type = PaymentOptionType.ContactDetails, Status = PaymentOptionStatus.PaymentPending});
                    }
                    else
                    {
                        flierQuery.PaymentOptions.RemoveWhere(_ => _.Type == PaymentOptionType.ContactDetails);
                    }
                }

                var flierStatus = FlierStatus.Active;
                if (flierQuery.PaymentOptions.Any(_ => _.Status == PaymentOptionStatus.PaymentPending))
                {
                    flierStatus = FlierStatus.PaymentPending;
                }
                _repository.UpdateEntity<Flier>(command.Id, 
                    flier =>
                        {
                            flier.FriendlyId = _queryService.FindFreeFriendlyId(flierQuery);
                            flier.Title = command.Title;
                            flier.Description = command.Description;
                            flier.Tags = command.Tags;
                            flier.Location = command.Location;
                            flier.Image = command.Image;
                            flier.EffectiveDate = command.EffectiveDate;
                            flier.ImageList = command.ImageList;
                            flier.PaymentOptions = flierQuery.PaymentOptions;
                            flier.UseBrowserContactDetails = attachContactDetails;
                            flier.Status = flierStatus;
                        });
                
                //add all existing board to the operation, as if a flier is modified it needs to be re-approved
                if (flierQuery.Boards != null)
                    command.BoardSet.UnionWith(flierQuery.Boards);
                
                boardFliers = AddFlierToBoardCommandHandler.UpdateAddFlierToBoards(command.BoardSet, flierQuery, _queryService,
                                                     _repository);           
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Flier Edit Failed", true)
                    .AddCommandId(command);

            _domainEventPublishService.Publish(
                new FlierModifiedEvent()
                    {
                        OrigState = flierQuery,
                        NewState = _queryService.FindById<Flier>(command.Id)
                    }
                );

            foreach (var boardFlierModifiedEvent in boardFliers)
            {
                _domainEventPublishService.Publish(boardFlierModifiedEvent);
            }

            return new MsgResponse("Flier Edit", false)
                .AddEntityId(command.Id)
                .AddCommandId(command);
        }
    }
}