using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Models.Browser;
using Website.Application.Binding;
using PostaFlya.Binding;
using Website.Application.Content;
using Website.Application.Domain.Browser.Web;
using Website.Application.Domain.Obsolete;
using Website.Common.Extension;
using Website.Common.Obsolete;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using PostaFlya.Models.Comments;
using Website.Domain.Comments;
using Website.Domain.Comments.Command;

namespace PostaFlya.Controllers
{
    public class CommentController : OldWebApiControllerBase
    {
        private readonly MessageBusInterface _messageBus;
        private readonly UnitOfWorkForRepoFactoryInterface _uowFactory;
        private readonly BlobStorageInterface _blobStorage;

        public CommentController(MessageBusInterface messageBus
            , UnitOfWorkForRepoFactoryInterface uowFactory
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _messageBus = messageBus;
            _uowFactory = uowFactory;
            _blobStorage = blobStorage;
        }

        //note shouldn't be accessing query service from controller
        private GenericQueryServiceInterface QueryService
        {
            get { return _uowFactory.GetUowInContext().CurrentQuery; }
        }

        [Website.Application.Domain.Obsolete.BrowserAuthorizeHttp(Roles = "Participant")]
        public HttpResponseMessage Post(CreateCommentModel commentCreateModel)
        {
            var entity = QueryService.FindById(ClaimController.GetTypeForClaimEntity(commentCreateModel.CommentEntity),
                commentCreateModel.EntityId) as EntityInterface;
            if (entity == null)
                return this.GetResponseForRes(new MsgResponse("Comment Failed", true)
                            .AddEntityIdError(commentCreateModel.EntityId));

            var commentCommand = new CreateCommentCommand()
                                          {
                                              BrowserId = commentCreateModel.BrowserId,
                                              Comment = commentCreateModel.Comment,
                                              CommentEntity = entity,
                                          };

            _messageBus.Send(commentCommand);
            return this.GetResponseForRes(new MsgResponse() { IsError = false });            

        }

        public IQueryable<CommentModel> Get(EntityTypeEnum entityTypeEnum, string id)
        {
//            return GetComments(_queryService, id)
//                .Select(c => c.FillBrowserModel(_queryService, _blobStorage));
            return GetComments(QueryService, id);
        }

        public static IQueryable<CommentModel> GetComments(GenericQueryServiceInterface commentQuery, string id)
        {
            if (commentQuery == null) return (new List<CommentModel>()).AsQueryable();
            return commentQuery.FindAggregateEntities<Comment>(id)
                .Select(c => c.ToViewModel());
        }

        private IList<CommentModel> GetDummtDaya(string id)
        {
            var browserId = "c49c12fa-1024-4bd6-aad4-1fd5967de296";
            IList<CommentInterface> list = new List<CommentInterface>()
                                               {
                                                   new Comment()
                                                       {
                                                           BrowserId = browserId,
                                                           AggregateId = id,
                                                           CommentContent = "This is a comment yo",
                                                           CommentTime = DateTime.UtcNow,
                                                           Id = "1"
                                                       },
                                                    new Comment()
                                                    {
                                                            BrowserId = browserId,
                                                           AggregateId = id,
                                                           CommentContent = "This is a comment yo",
                                                           CommentTime = DateTime.UtcNow,
                                                           Id = "2"
                                                    },
                                                    new Comment()
                                                       {
                                                           BrowserId = browserId,
                                                           AggregateId = id,
                                                           CommentContent = "This is a comment yo",
                                                           CommentTime = DateTime.UtcNow,
                                                           Id = "3"
                                                       },
                                                    new Comment()
                                                    {
                                                           BrowserId = browserId,
                                                           AggregateId = id,
                                                           CommentContent = "This is a comment yo",
                                                           CommentTime = DateTime.UtcNow,
                                                           Id = "4"
                                                    }
                                               };
            return list.Select(c => c.ToViewModel(QueryService, _blobStorage)).ToList();
        }
    }
}