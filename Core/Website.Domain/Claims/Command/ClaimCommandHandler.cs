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

        public ClaimCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , GenericRepositoryInterface genericRepository
            , GenericQueryServiceInterface genericQueryService
            , DomainEventPublishServiceInterface domainEventPublishService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRepository = genericRepository;
            _genericQueryService = genericQueryService;
            _domainEventPublishService = domainEventPublishService;
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
                Id = command.ClaimEntity.Id + browser.Id,
                AggregateId = command.ClaimEntity.Id,
                BrowserId = browser.Id,
                ClaimContext = command.Context,
                ClaimTime = DateTime.UtcNow,            
            };

            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                _genericRepository.Store(claim); 
            }

            if(!uow.Successful)
                return new MsgResponse("Claim Entity failed", true)
                .AddCommandId(command);

            
            
            uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                
                _genericRepository.UpdateEntity(command.ClaimEntity.GetType()
                , command.ClaimEntity.Id
                , o =>
                {
                    var com = o as ClaimableInterface;
                    if (com != null)
                        com.NumberOfClaims = _genericQueryService.FindAggregateEntities<Claim>(claim.AggregateId).Count();
                });

            }

            if (!uow.Successful)
                return new MsgResponse("Claim Entity failed", true)
                        .AddCommandId(command);

            if (command.OwnerEntity is ChargableEntityInterface)
            {
                HandleChargableEntity(command.OwnerEntity as ChargableEntityInterface, uow, command.ClaimEntity as ClaimableInterface);
                if (!uow.Successful)
                {
                    return new MsgResponse("Charging Claim Entity failed", true).AddCommandId(command);
                }
            }

         
            _domainEventPublishService.Publish(new ClaimEvent(){NewState = claim});

            return new MsgResponse("Claim Entity", false)
             .AddCommandId(command)
             .AddEntityId(claim.AggregateId);

        }

        public UnitOfWorkInterface HandleChargableEntity(ChargableEntityInterface ownerEntity, UnitOfWorkInterface uow, ClaimableInterface claimEnitity)
        {
            uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {

                _genericRepository.UpdateEntity(ownerEntity.GetType()
                , ownerEntity.Id
                , o =>
                    {
                        var chargeable = o as ChargableEntityInterface;
                        var claimable = claimEnitity as ClaimableInterface;


                        if (chargeable != null) chargeable.AccountCredit -= claimable.ClaimCost;
                    });

            }

            return uow;
        }

    }
}