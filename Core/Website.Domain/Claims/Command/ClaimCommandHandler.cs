using System;
using System.Collections.Generic;
using System.Linq;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using Website.Domain.Browser;
using Website.Domain.Service;

//using Website.Infrastructure.Service;

namespace Website.Domain.Claims.Command
{
    internal class ClaimCommandHandler : MessageHandlerInterface<ClaimCommand>
    {
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;
        private readonly CreditChargeServiceInterface _creditChargeService;

        public ClaimCommandHandler(GenericRepositoryInterface genericRepository
            , GenericQueryServiceInterface genericQueryService, CreditChargeServiceInterface creditChargeService)
        {
            _genericRepository = genericRepository;
            _genericQueryService = genericQueryService;
            _creditChargeService = creditChargeService;
        }

        public void Handle(ClaimCommand command)
        {
            var browser = _genericQueryService.FindById<Browser.Browser>(command.BrowserId); 

            if(browser == null || !browser.HasRole(Role.Participant))
            {
                return;
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


            claim.ChargeForState(_genericRepository, _genericQueryService, _creditChargeService);
            _genericRepository.Store(claim); 
                    
            _genericRepository.UpdateEntity(command.ClaimEntity.GetType()
            , command.ClaimEntity.Id
            , o =>
            {
                var com = o as ClaimableEntityInterface;
                if (com != null)
                    com.NumberOfClaims = _genericQueryService.FindAggregateEntities<Claim>(claim.AggregateId).Count();
            });

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