using System;
using System.Collections.Generic;
using System.Linq;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using Website.Domain.Browser;

//using Website.Infrastructure.Service;

namespace Website.Domain.Comments.Command
{
    internal class CreateCommentCommandHandler : MessageHandlerInterface<CreateCommentCommand>
    {
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;

        public CreateCommentCommandHandler(GenericRepositoryInterface genericRepository
            , GenericQueryServiceInterface genericQueryService)
        {
            _genericRepository = genericRepository;
            _genericQueryService = genericQueryService;
        }

        public void Handle(CreateCommentCommand command)
        {
            var browser = _genericQueryService.FindById<Browser.Browser>(command.BrowserId);

            if (browser == null || !browser.HasRole(Role.Participant)
                || string.IsNullOrWhiteSpace(command.Comment))
            {
                return;
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

            _genericRepository.Store(comment);     
            
            _genericRepository.UpdateEntity(command.CommentEntity.GetType()
                , command.CommentEntity.Id
                , o =>
                {
                    var com = o as CommentableInterface;
                    if (com != null)
                        com.NumberOfComments = _genericQueryService.FindAggregateEntities<Comment>(comment.AggregateId).Count();
                });


        }
    }
}