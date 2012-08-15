using System;
using System.Collections.Generic;
using Ninject;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Flier.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using WebSite.Infrastructure.Service;

namespace PostaFlya.Domain.Likes.Command
{
    internal class LikeCommandHandler : CommandHandlerInterface<LikeCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly GenericServiceFactoryInterface _genericServiceFactory;

        public LikeCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , BrowserQueryServiceInterface browserQueryService
            , GenericServiceFactoryInterface genericServiceFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _browserQueryService = browserQueryService;
            _genericServiceFactory = genericServiceFactory;
        }

        public object Handle(LikeCommand command)
        {
            var repository =
                GetRepositoryForLikeEntity(command.LikeEntity);

            var browser = _browserQueryService.FindById(command.BrowserId); 

            if(repository == null || browser == null || !browser.HasRole(Role.Participant))
            {
                return new MsgResponse("Like Entity failed", true)
                    .AddCommandId(command);
            }

            var like = new Like()
            {              
                EntityId = command.LikeEntity.Id,
                BrowserId = browser.Id,
                LikeContent = command.Comment,
                ILike = command.ILike,
                LikeTime = DateTime.UtcNow
            };

            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() {repository});
            using (uow)
            {
                if (repository.Like(like) == null)
                    return new MsgResponse("Like Entity failed", true)
                        .AddCommandId(command)
                        .AddEntityIdError(like.EntityId);
            }

            if (uow.Successful)
                return new MsgResponse("Like Entity", false)
                    .AddCommandId(command)
                    .AddEntityId(like.EntityId);

            return new MsgResponse("Like Entity failed", true)
                        .AddCommandId(command);
        }

        private readonly Type _likeServiceTyp = typeof(AddLikeInterface<>);
        private AddLikeInterface GetRepositoryForLikeEntity(EntityInterface commentEntity)
        {
            var ret =
                _genericServiceFactory.FindService<AddLikeInterface>
                    (_likeServiceTyp, commentEntity.PrimaryInterface);

            return ret;
        }
    }
}