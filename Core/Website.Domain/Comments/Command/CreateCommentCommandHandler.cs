using System;
using System.Collections.Generic;
using System.Linq;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;

//using WebSite.Infrastructure.Service;

namespace Website.Domain.Comments.Command
{
    internal class CreateCommentCommandHandler : CommandHandlerInterface<CreateCommentCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;

        public CreateCommentCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , BrowserQueryServiceInterface browserQueryService
            , GenericRepositoryInterface genericRepository
            , GenericQueryServiceInterface genericQueryService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _browserQueryService = browserQueryService;
            _genericRepository = genericRepository;
            _genericQueryService = genericQueryService;
        }

        public object Handle(CreateCommentCommand command)
        {
            var browser = _browserQueryService.FindById<Browser.Browser>(command.BrowserId);

            if (browser == null || !browser.HasRole(Role.Participant)
                || string.IsNullOrWhiteSpace(command.Comment))
            {
                return new MsgResponse("Comment Failed", true)
                    .AddCommandId(command);
            }

            var comment = new Comment()
            {
                Id = Guid.NewGuid().ToString(),
                CommentContent = command.Comment,
                BrowserId = command.BrowserId,
                AggregateId = command.CommentEntity.Id,
                CommentTime = DateTime.UtcNow,
            };

            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                _genericRepository.Store(comment);     
            }

            if (!uow.Successful)
            return new MsgResponse("Comment Failed", true)
                .AddCommandId(command); 

            uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                _genericRepository.UpdateEntity(command.CommentEntity.GetType()
                    , command.CommentEntity.Id
                    , o =>
                    {
                        var com = o as CommentableInterface;
                        if (com != null)
                            com.NumberOfComments = _genericQueryService.FindAggregateEntities<Comment>(comment.AggregateId).Count();
                    });
            }

            if(uow.Successful)
                return new MsgResponse("Comment Create", false)
                    .AddEntityId(comment.AggregateId)
                    .AddCommandId(command);

            return new MsgResponse("Comment Failed", true)
                    .AddCommandId(command); 

        }
    }
}