using System;
using System.Diagnostics;
using System.Linq;
using PostaFlya.Domain.Boards;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Domain.Flier.Command
{
    internal class EditFlierCommandHandler : MessageHandlerInterface<EditFlierCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly CreditChargeServiceInterface _creditChargeService;


        public EditFlierCommandHandler(GenericRepositoryInterface repository
            ,UnitOfWorkFactoryInterface unitOfWorkFactory
            , GenericQueryServiceInterface queryService, CreditChargeServiceInterface creditChargeService)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _queryService = queryService;
            _creditChargeService = creditChargeService;
        }

        public object Handle(EditFlierCommand command)
        {
            var flierQuery = _queryService.FindById<Flier>(command.Id);
            if (flierQuery == null || flierQuery.BrowserId == null || 
                !flierQuery.BrowserId.Equals(command.BrowserId))
                return false;

            UnitOfWorkInterface unitOfWork;

            var eventDates =
                command.EventDates.Select(d => d.SetOffsetMinutes(command.Venue != null ? command.Venue.UtcOffset : 0)).ToList();
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {
                //

                _repository.UpdateEntity<Flier>(command.Id, 
                    flier =>
                        {
                            flier.Title = command.Title;
                            flier.Description = command.Description;
                            flier.Tags = command.Tags;
                            flier.Image = command.Image;
                            flier.EventDates = eventDates;
                            flier.ImageList = command.ImageList;
                            flier.LocationRadius = command.ExtendPostRadius;
                            flier.HasLeadGeneration = command.AllowUserContact;
                            flier.EnableAnalytics = command.EnableAnalytics;
                            flier.Status = FlierStatus.Pending;
                            flier.Features = CreateFlierCommandHandler.GetPaymentFeatures(flier);
                            flier.MergeUpdateFeatureCharges(flierQuery.Features);
                            flier.UserLinks = command.UserLinks;
                            if (flierQuery.Boards != null)
                            {
                                flier.Boards =
                                    flier.Boards.Union(
                                        command.BoardSet.Select(
                                            _ => new BoardFlier() {BoardId = _, DateAdded = DateTime.UtcNow})).ToList();
                            }
                        });
                      
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Flier Edit Failed", true)
                    .AddCommandId(command);

            /*using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository}))
            {
                //add all existing board to the operation, as if a flier is modified it needs to be re-approved
                if (flierQuery.Boards != null)
                    command.BoardSet.UnionWith(flierQuery.Boards);

                boardFliers = AddFlierToBoardCommandHandler.UpdateAddFlierToBoards(command.BoardSet, flierQuery, _queryService,
                                                     _repository);     
            }*/


            //charge for any new state
            bool enabled = false;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository}))
            {
                var flierCurrent = _queryService.FindById<Flier>(command.Id);
                enabled = flierCurrent.ChargeForState(_repository, _queryService, _creditChargeService);
                _repository.UpdateEntity<Flier>(flierCurrent.Id, f =>
                {
                    f.MergeUpdateFeatureCharges(flierCurrent.Features); 
                    f.Status = enabled ? FlierStatus.Active : FlierStatus.PaymentPending;
                });

            }

            if(!unitOfWork.Successful)
                Trace.TraceError("Error charging for flier features");
            

            return new MsgResponse("Flier Edit", false)
                .AddEntityId(command.Id)
                .AddCommandId(command)
                .AddMessageProperty("status", enabled ? FlierStatus.Active.ToString() : FlierStatus.PaymentPending.ToString());
        }
    }
}