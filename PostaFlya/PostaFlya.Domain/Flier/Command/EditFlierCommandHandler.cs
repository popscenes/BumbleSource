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
        private readonly GenericQueryServiceInterface _queryService;
        private readonly CreditChargeServiceInterface _creditChargeService;


        public EditFlierCommandHandler(GenericRepositoryInterface repository
            , GenericQueryServiceInterface queryService, CreditChargeServiceInterface creditChargeService)
        {
            _repository = repository;
            _queryService = queryService;
            _creditChargeService = creditChargeService;
        }

        public void Handle(EditFlierCommand command)
        {
            var flierQuery = _queryService.FindById<Flier>(command.Id);
            if (flierQuery == null || flierQuery.BrowserId == null || 
                !flierQuery.BrowserId.Equals(command.BrowserId))
                return;

            UnitOfWorkInterface unitOfWork;

            var eventDates =
                command.EventDates.Select(d => d.SetOffsetMinutes(command.Venue != null ? command.Venue.UtcOffset : 0)).ToList();

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

            //charge for any new state
            bool enabled = false;

            var flierCurrent = _queryService.FindById<Flier>(command.Id);
            enabled = flierCurrent.ChargeForState(_repository, _queryService, _creditChargeService);
            _repository.UpdateEntity<Flier>(flierCurrent.Id, f =>
            {
                f.MergeUpdateFeatureCharges(flierCurrent.Features); 
                f.Status = enabled ? FlierStatus.Active : FlierStatus.PaymentPending;
            });

        }
    }
}