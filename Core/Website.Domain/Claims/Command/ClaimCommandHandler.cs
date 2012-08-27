using System;
using System.Collections.Generic;
using System.Linq;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;

//using WebSite.Infrastructure.Service;

namespace Website.Domain.Claims.Command
{
    internal class ClaimCommandHandler : CommandHandlerInterface<ClaimCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;

        public ClaimCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , BrowserQueryServiceInterface browserQueryService
            , GenericRepositoryInterface genericRepository
            , GenericQueryServiceInterface genericQueryService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _browserQueryService = browserQueryService;
            _genericRepository = genericRepository;
            _genericQueryService = genericQueryService;
        }

        public object Handle(ClaimCommand command)
        {
            var browser = _browserQueryService.FindById<Browser.Browser>(command.BrowserId); 

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
                ClaimContext = command.Comment,
                ClaimTime = DateTime.UtcNow
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


            if (uow.Successful)
                return new MsgResponse("Claim Entity", false)
                    .AddCommandId(command)
                    .AddEntityId(claim.AggregateId);

            return new MsgResponse("Claim Entity failed", true)
                        .AddCommandId(command);
        }
    }
}