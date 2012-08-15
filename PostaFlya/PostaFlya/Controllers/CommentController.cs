using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebSite.Application.Binding;
using PostaFlya.Application.Domain.Content;
using PostaFlya.Attributes;
using PostaFlya.Binding;
using WebSite.Application.Content;
using WebSite.Common.Extension;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Comments.Command;
using PostaFlya.Domain.Comments.Query;
using PostaFlya.Domain.Flier.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Comments;
using PostaFlya.Models.Flier;

namespace PostaFlya.Controllers
{
    public class CommentController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly EntityQueryServiceFactoryInterface _entityQueryServiceFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly BlobStorageInterface _blobStorage;

        public CommentController(CommandBusInterface commandBus
            , EntityQueryServiceFactoryInterface entityQueryServiceFactory
            , BrowserQueryServiceInterface browserQueryService
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _commandBus = commandBus;
            _entityQueryServiceFactory = entityQueryServiceFactory;
            _browserQueryService = browserQueryService;
            _blobStorage = blobStorage;
        }

        [BrowserAuthorize(Roles = "Participant")]
        public HttpResponseMessage Post(CreateCommentModel commentCreateModel)
        {
            var qs = _entityQueryServiceFactory
                .GetQueryServiceForEntityTyp<QueryServiceInterface>
                (commentCreateModel.CommentEntity);

            if(qs == null)
                return this.GetResponseForRes(new MsgResponse("Comment Failed", true));

            var entity = qs.FindById(commentCreateModel.EntityId) as EntityInterface;
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
            var qs = _entityQueryServiceFactory.GetQueryServiceForEntityTyp<QueryServiceInterface>(entityTypeEnum);
            return GetComments(qs as QueryCommentsInterface, id)
                .Select(c => c.FillBrowserModel(_browserQueryService, _blobStorage));
        }

        public static IQueryable<CommentModel> GetComments(QueryCommentsInterface commentQuery, string id)
        {
            if (commentQuery == null) return (new List<CommentModel>()).AsQueryable();
            return commentQuery.GetComments(id)
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
                                                           EntityId = id,
                                                           CommentContent = "This is a comment yo",
                                                           CommentTime = DateTime.UtcNow,
                                                           Id = "1"
                                                       },
                                                    new Comment()
                                                    {
                                                            BrowserId = browserId,
                                                           EntityId = id,
                                                           CommentContent = "This is a comment yo",
                                                           CommentTime = DateTime.UtcNow,
                                                           Id = "2"
                                                    },
                                                    new Comment()
                                                       {
                                                           BrowserId = browserId,
                                                           EntityId = id,
                                                           CommentContent = "This is a comment yo",
                                                           CommentTime = DateTime.UtcNow,
                                                           Id = "3"
                                                       },
                                                    new Comment()
                                                    {
                                                           BrowserId = browserId,
                                                           EntityId = id,
                                                           CommentContent = "This is a comment yo",
                                                           CommentTime = DateTime.UtcNow,
                                                           Id = "4"
                                                    }
                                               };
            return list.Select(c => c.ToViewModel(_browserQueryService, _blobStorage)).ToList();
        }
    }
}