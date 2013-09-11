using System;
using System.Collections.Generic;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier.Payment;
using PostaFlya.Domain.Flier.Query;
using Website.Domain.Payment;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using System.Linq;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Domain.Flier.Command
{
    internal class CreateFlierCommandHandler : MessageHandlerInterface<CreateFlierCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericQueryServiceInterface _flierQueryService;
        private readonly CreditChargeServiceInterface _creditChargeService;
        private readonly TinyUrlServiceInterface _tinyUrlService;
        private readonly QueryChannelInterface _queryChannel;

        public CreateFlierCommandHandler(GenericRepositoryInterface repository
            , UnitOfWorkFactoryInterface unitOfWorkFactory, GenericQueryServiceInterface flierQueryService, CreditChargeServiceInterface creditChargeService
            , TinyUrlServiceInterface tinyUrlService, QueryChannelInterface queryChannel)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _flierQueryService = flierQueryService;
            _creditChargeService = creditChargeService;
            _tinyUrlService = tinyUrlService;
            _queryChannel = queryChannel;
        }

        public object Handle(CreateFlierCommand command)
        {
            var date = DateTime.UtcNow;

            var venueBoard = _queryChannel.Query(new FindByIdsQuery<Board>() { Ids = command.BoardSet.ToList() }, new List<Board>())
                .FirstOrDefault(board => board.Venue() != null);

            var eventDates =
                    command.EventDates.Select(d => d.SetOffsetMinutes(venueBoard != null ? venueBoard.Venue().UtcOffset : 0)).ToList();

            var newFlier = new Flier()
                               {
                                   BrowserId = command.Anonymous ? Guid.Empty.ToString() : command.BrowserId,
                                   Title = command.Title,
                                   Description = command.Description,
                                   Tags = command.Tags,
                                   Image = command.Image,
                                   CreateDate = date,
                                   FlierBehaviour = command.FlierBehaviour,
                                   EventDates = eventDates,                            
                                   ImageList = command.ImageList,
                                   ExternalSource = command.ExternalSource,
                                   ExternalId = command.ExternalId,
                                   LocationRadius = command.ExtendPostRadius,
                                   HasLeadGeneration = command.AllowUserContact,
                                   EnableAnalytics = command.EnableAnalytics,
                                   Status = FlierStatus.Pending,
                                   UserLinks = command.UserLinks
                               };

            newFlier.Boards =
                command.BoardSet.Select(
                    _ => new BoardFlier() { BoardId = _, DateAdded = DateTime.Now, Status = BoardFlierStatus.Approved })
                    .ToList();

            newFlier.FriendlyId = _queryChannel.FindFreeFriendlyIdForFlier(newFlier);
            newFlier.Features = GetPaymentFeatures(newFlier);
            newFlier.TinyUrl = _tinyUrlService.UrlFor(newFlier);



            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {
                var enabled = command.Anonymous || newFlier.ChargeForState(_repository, _flierQueryService, _creditChargeService);
                newFlier.Status = enabled ? FlierStatus.Active : FlierStatus.PaymentPending;
                _repository.Store(newFlier);
            }

            if(!unitOfWork.Successful)
                return new MsgResponse("Flier Creation Failed", true)
                        .AddCommandId(command);
            
            return new MsgResponse("Flier Create", false)
                .AddEntityId(newFlier.Id)
                .AddCommandId(command)
                .AddMessageProperty("status", newFlier.Status.ToString());
        }

        public static HashSet<EntityFeatureCharge>  GetPaymentFeatures(FlierInterface newFlier)
        {
            var  features = new HashSet<EntityFeatureCharge>
                {
                    PostFlierFlatFeeChargeBehaivour.GetPostRadiusFeatureCharge()
                };

            if (newFlier.HasLeadGeneration)
                features.Add(LeadGenerationFeatureChargeBehaviour.GetLeadGenerationFeatureCharge());
            
            if(newFlier.EnableAnalytics)
                features.Add(AnalyticsFeatureChargeBehaviour.GetAnalyticsFeatureChargeBehaviour());

            return features;
        }
        
    }
}