using System;
using System.Collections.Generic;
using System.Linq;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;

//using WebSite.Infrastructure.Service;

namespace Website.Domain.Likes.Command
{
    internal class LikeCommandHandler : CommandHandlerInterface<LikeCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;

        public LikeCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , BrowserQueryServiceInterface browserQueryService
            , GenericRepositoryInterface genericRepository
            , GenericQueryServiceInterface genericQueryService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _browserQueryService = browserQueryService;
            _genericRepository = genericRepository;
            _genericQueryService = genericQueryService;
        }

        public object Handle(LikeCommand command)
        {
            var browser = _browserQueryService.FindById<Browser.Browser>(command.BrowserId); 

            if(browser == null || !browser.HasRole(Role.Participant))
            {
                return new MsgResponse("Like Entity failed", true)
                    .AddCommandId(command);
            }

            var like = new Like()
            {    
                Id = command.LikeEntity.Id + browser.Id,
                AggregateId = command.LikeEntity.Id,
                BrowserId = browser.Id,
                LikeContent = command.Comment,
                ILike = command.ILike,
                LikeTime = DateTime.UtcNow
            };

            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                _genericRepository.Store(like); 
            }

            if(!uow.Successful)
                return new MsgResponse("Like Entity failed", true)
                .AddCommandId(command);

            uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                _genericRepository.UpdateEntity(command.LikeEntity.GetType()
                , command.LikeEntity.Id
                , o =>
                {
                    var com = o as LikeableInterface;
                    if (com != null)
                        com.NumberOfLikes = _genericQueryService.FindAggregateEntities<Like>(like.AggregateId).Count();
                }); 
            }


            if (uow.Successful)
                return new MsgResponse("Like Entity", false)
                    .AddCommandId(command)
                    .AddEntityId(like.AggregateId);

            return new MsgResponse("Like Entity failed", true)
                        .AddCommandId(command);
        }
    }
}