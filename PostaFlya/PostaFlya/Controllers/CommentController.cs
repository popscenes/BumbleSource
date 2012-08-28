using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Website.Application.Binding;
using PostaFlya.Attributes;
using PostaFlya.Binding;
using Website.Application.Content;
using Website.Common.Extension;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Comments;
using Website.Domain.Browser.Query;
using Website.Domain.Comments;
using Website.Domain.Comments.Command;

namespace PostaFlya.Controllers
{
    public class CommentController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _entityQueryService;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly GenericQueryServiceInterface _queryComments;
        private readonly BlobStorageInterface _blobStorage;

        public CommentController(CommandBusInterface commandBus
            , GenericQueryServiceInterface entityQueryService
            , BrowserQueryServiceInterface browserQueryService
            , GenericQueryServiceInterface queryComments
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _commandBus = commandBus;
            _entityQueryService = entityQueryService;
            _browserQueryService = browserQueryService;
            _queryComments = queryComments;
            _blobStorage = blobStorage;
        }

        [BrowserAuthorize(Roles = "Participant")]
        public HttpResponseMessage Post(CreateCommentModel commentCreateModel)
        {
            var entity = _entityQueryService.FindById(ClaimController.GetTypeForClaimEntity(commentCreateModel.CommentEntity),
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

            var res = _commandBus.Send(commentCommand);
            return this.GetResponseForRes(res);
        }

        [Queryable]
        public IQueryable<CommentModel> Get(EntityTypeEnum entityTypeEnum, string id)
        {
            return GetComments(_queryComments, id)
                .Select(c => c.FillBrowserModel(_browserQueryService, _blobStorage));
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
            return list.Select(c => c.ToViewModel(_browserQueryService, _blobStorage)).ToList();
        }
    }
}