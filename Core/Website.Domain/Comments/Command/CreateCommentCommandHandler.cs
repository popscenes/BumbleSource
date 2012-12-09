using System;
using System.Collections.Generic;
using System.Linq;
using Website.Domain.Comments.Event;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Domain.Browser;

//using Website.Infrastructure.Service;

namespace Website.Domain.Comments.Command
{
    internal class CreateCommentCommandHandler : CommandHandlerInterface<CreateCommentCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;

        public CreateCommentCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , GenericRepositoryInterface genericRepository
            , GenericQueryServiceInterface genericQueryService
            , DomainEventPublishServiceInterface domainEventPublishService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRepository = genericRepository;
            _genericQueryService = genericQueryService;
            _domainEventPublishService = domainEventPublishService;
        }

        public object Handle(CreateCommentCommand command)
        {
            var browser = _genericQueryService.FindById<Browser.Browser>(command.BrowserId);

            if (browser == null || !browser.HasRole(Role.Participant)
                || string.IsNullOrWhiteSpace(command.Comment))
            {
                return new MsgResponse("Comment Failed", true)
                    .AddCommandId(command);
            }

            var comment = new Comment()
            {
                CommentTime = DateTime.UtcNow,
                CommentContent = command.Comment,
                BrowserId = command.BrowserId,
                AggregateId = command.CommentEntity.Id,
                AggregateTypeTag = command.CommentEntity.GetType().Name 
            };
            comment.SetId();

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

            if(!uow.Successful)
                return new MsgResponse("Comment Failed", true)
                   .AddCommandId(command); 

            _domainEventPublishService.Publish(new CommentEvent(){NewState = comment});

            return new MsgResponse("Comment Create", false)
                .AddEntityId(comment.AggregateId)
                .AddCommandId(command);

        }
    }
}