using System;
using System.Collections.Generic;
using Ninject;
using Ninject.Syntax;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Flier.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using WebSite.Infrastructure.Service;

namespace PostaFlya.Domain.Comments.Command
{
    internal class CreateCommentCommandHandler : CommandHandlerInterface<CreateCommentCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly GenericServiceFactoryInterface _genericServiceFactory;

        public CreateCommentCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , BrowserQueryServiceInterface browserQueryService
            , GenericServiceFactoryInterface genericServiceFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _browserQueryService = browserQueryService;
            _genericServiceFactory = genericServiceFactory;
        }

        public object Handle(CreateCommentCommand command)
        {
            var repository = GetRepositoryForCommentEntity(command.CommentEntity);

            var browser = _browserQueryService.FindById(command.BrowserId);

            if (repository == null || browser == null || !browser.HasRole(Role.Participant)
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
                EntityId = command.CommentEntity.Id,
                CommentTime = DateTime.UtcNow,
            };

            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() {repository});
            using (uow)
            {
                var ret = repository.AddComment(comment);
                if(ret == null)
                    return new MsgResponse("Comment Failed", true)
                        .AddCommandId(command).AddEntityId(comment.EntityId);             
            }

            if(uow.Successful)
                return new MsgResponse("Flier Comment Create", false)
                    .AddEntityId(comment.EntityId)
                    .AddCommandId(command);

            return new MsgResponse("Comment Failed", true)
                    .AddCommandId(command); 

        }

        private readonly Type _commentServiceTyp = typeof (AddCommentInterface<>);
        private AddCommentInterface GetRepositoryForCommentEntity(EntityInterface commentEntity)
        {
            var ret =
                _genericServiceFactory.FindService<AddCommentInterface>
                    (_commentServiceTyp, commentEntity.PrimaryInterface);

            return ret;
        }
    }
}