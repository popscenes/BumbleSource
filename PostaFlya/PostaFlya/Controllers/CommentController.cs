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
using Website.Common.Extension;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using PostaFlya.Models.Comments;
using Website.Domain.Comments;
using Website.Domain.Comments.Command;

namespace PostaFlya.Controllers
{
    public class CommentController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;

        public CommentController(CommandBusInterface commandBus
            , GenericQueryServiceInterface queryService
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _commandBus = commandBus;
            _queryService = queryService;
            _blobStorage = blobStorage;
        }

        [BrowserAuthorize(Roles = "Participant")]
        public HttpResponseMessage Post(CreateCommentModel commentCreateModel)
        {
            var entity = _queryService.FindById(ClaimController.GetTypeForClaimEntity(commentCreateModel.CommentEntity),
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

        public IQueryable<CommentModel> Get(EntityTypeEnum entityTypeEnum, string id)
        {
            return GetComments(_queryService, id)
                .Select(c => c.FillBrowserModel(_queryService, _blobStorage));
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
            return list.Select(c => c.ToViewModel(_queryService, _blobStorage)).ToList();
        }
    }
}