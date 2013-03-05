using System;
using System.Collections.Generic;
using System.Linq;
using Website.Domain.Claims.Event;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Domain.Browser;
using Website.Domain.Service;

//using Website.Infrastructure.Service;

namespace Website.Domain.Claims.Command
{
    internal class ClaimCommandHandler : CommandHandlerInterface<ClaimCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;
        private readonly CreditChargeServiceInterface _creditChargeService;

        public ClaimCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , GenericRepositoryInterface genericRepository
            , GenericQueryServiceInterface genericQueryService
            , DomainEventPublishServiceInterface domainEventPublishService
            , CreditChargeServiceInterface creditChargeService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRepository = genericRepository;
            _genericQueryService = genericQueryService;
            _domainEventPublishService = domainEventPublishService;
            _creditChargeService = creditChargeService;
        }

        public object Handle(ClaimCommand command)
        {
            var browser = _genericQueryService.FindById<Browser.Browser>(command.BrowserId); 

            if(browser == null || !browser.HasRole(Role.Participant))
            {
                return new MsgResponse("Claim Entity failed", true)
                    .AddCommandId(command);
            }

            var claim = new Claim()
            {    
                AggregateId = command.ClaimEntity.Id,
                AggregateTypeTag = command.ClaimEntity.GetType().Name,
                BrowserId = browser.Id,
                ClaimContext = command.Context,
                ClaimTime = DateTime.UtcNow,
                ClaimMessage = command.Message
            };

            claim.Id = claim.GetId();

            var entityFeaturesCharges = command.ClaimEntity as EntityFeatureChargesInterface;
            if (entityFeaturesCharges != null)
                entityFeaturesCharges.MergeChargesForAggregateMemberEntity(claim);

            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                claim.ChargeForState(_genericRepository, _genericQueryService, _creditChargeService);
                _genericRepository.Store(claim); 
            }

            if(!uow.Successful)
                return new MsgResponse("Claim Entity failed", true)
                .AddCommandId(command);


            _domainEventPublishService.Publish(new ClaimEvent() { NewState = claim });
            
            
            uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                
                _genericRepository.UpdateEntity(command.ClaimEntity.GetType()
                , command.ClaimEntity.Id
                , o =>
                {
                    var com = o as ClaimableEntityInterface;
                    if (com != null)
                        com.NumberOfClaims = _genericQueryService.FindAggregateEntities<Claim>(claim.AggregateId).Count();
                });

            }

            if (!uow.Successful)
                return new MsgResponse("Claim Entity failed", true)
                        .AddCommandId(command);

//            if (command.OwnerEntity is ChargableEntityInterface)
//            {
//                HandleChargableEntity(command.OwnerEntity as ChargableEntityInterface, uow, command.ClaimEntity as ClaimableEntityInterface, claim);
//                if (!uow.Successful)
//                {
//                    return new MsgResponse("Charging Claim Entity failed", true).AddCommandId(command);
//                }
//            }

            return new MsgResponse("Claim Entity", false)
             .AddCommandId(command)
             .AddEntityId(claim.AggregateId);

        }

//        public UnitOfWorkInterface HandleChargableEntity(ChargableEntityInterface ownerEntity, UnitOfWorkInterface uow, ClaimableEntityInterface claimEnitity, Claim claim)
//        {
//            uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
//            using (uow)
//            {
//
//                _genericRepository.UpdateEntity(ownerEntity.GetType()
//                , ownerEntity.Id
//                , o =>
//                    {
//                        var chargeable = o as ChargableEntityInterface;
//                        var claimable = claimEnitity as ClaimableEntityInterface;
//
//
//                        if (chargeable != null) chargeable.AccountCredit -= claimable.ClaimCost(claim);
//                    });
//
//            }
//
//            return uow;
//        }

    }
}