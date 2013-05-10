using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Flier.Event;
using PostaFlya.Domain.Flier.Query;
using Website.Domain.Payment;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Domain.Flier.Command
{
    internal class EditFlierCommandHandler : CommandHandlerInterface<EditFlierCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;
        private readonly CreditChargeServiceInterface _creditChargeServiceInterface;


        public EditFlierCommandHandler(GenericRepositoryInterface repository
            ,UnitOfWorkFactoryInterface unitOfWorkFactory
            , GenericQueryServiceInterface queryService, DomainEventPublishServiceInterface domainEventPublishService
            , CreditChargeServiceInterface creditChargeServiceInterface)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _queryService = queryService;
            _domainEventPublishService = domainEventPublishService;
            _creditChargeServiceInterface = creditChargeServiceInterface;
        }

        public object Handle(EditFlierCommand command)
        {
            var flierQuery = _queryService.FindById<Flier>(command.Id);
            if (flierQuery == null || flierQuery.BrowserId == null || 
                !flierQuery.BrowserId.Equals(command.BrowserId))
                return false;

            List<BoardFlierModifiedEvent> boardFliers = null;
            UnitOfWorkInterface unitOfWork;

            var eventDates =
                command.EventDates.Select(d => d.SetOffsetMinutes(command.ContactDetails != null ? command.ContactDetails.UtcOffset : 0)).ToList();
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {
                //

                _repository.UpdateEntity<Flier>(command.Id, 
                    flier =>
                        {
                            flier.Title = command.Title;
                            flier.Description = command.Description;
                            flier.Tags = command.Tags;
                            flier.Location = command.Location;
                            flier.Image = command.Image;
                            flier.EventDates = eventDates;
                            flier.ImageList = command.ImageList;
                            flier.LocationRadius = command.ExtendPostRadius;
                            flier.HasLeadGeneration = command.AllowUserContact;
                            flier.EnableAnalytics = command.EnableAnalytics;
                            flier.Status = FlierStatus.Pending;
                            flier.Features = CreateFlierCommandHandler.GetPaymentFeatures(flier);
                            flier.MergeUpdateFeatureCharges(flierQuery.Features);
                            flier.ContactDetails = command.ContactDetails;
                            flier.UserLinks = command.UserLinks;
                        });
                      
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Flier Edit Failed", true)
                    .AddCommandId(command);

            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository}))
            {
                //add all existing board to the operation, as if a flier is modified it needs to be re-approved
                if (flierQuery.Boards != null)
                    command.BoardSet.UnionWith(flierQuery.Boards);

                boardFliers = AddFlierToBoardCommandHandler.UpdateAddFlierToBoards(command.BoardSet, flierQuery, _queryService,
                                                     _repository);     
            }


            //charge for any new state
            bool enabled = false;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository}))
            {
                var flierCurrent = _queryService.FindById<Flier>(command.Id);
                enabled = flierCurrent.ChargeForState(_repository, _queryService, _creditChargeServiceInterface);
                _repository.UpdateEntity<Flier>(flierCurrent.Id, f =>
                {
                    f.MergeUpdateFeatureCharges(flierCurrent.Features); 
                    f.Status = enabled ? FlierStatus.Active : FlierStatus.PaymentPending;
                });

            }

            if(!unitOfWork.Successful)
                Trace.TraceError("Error charging for flier features");
            

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
                .AddCommandId(command)
                .AddMessageProperty("status", enabled ? FlierStatus.Active.ToString() : FlierStatus.PaymentPending.ToString());
        }
    }
}