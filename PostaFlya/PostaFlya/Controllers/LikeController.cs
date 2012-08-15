using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebSite.Application.Binding;
using PostaFlya.Application.Domain.Content;
using PostaFlya.Binding;
using WebSite.Application.Content;
using WebSite.Common.Extension;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Likes;
using PostaFlya.Domain.Likes.Command;
using PostaFlya.Domain.Likes.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Likes;

namespace PostaFlya.Controllers
{
    public class LikeController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly EntityQueryServiceFactoryInterface _entityQueryServiceFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly BlobStorageInterface _blobStorage;

        public LikeController(CommandBusInterface commandBus
            , EntityQueryServiceFactoryInterface entityQueryServiceFactory
            , BrowserQueryServiceInterface browserQueryService
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _commandBus = commandBus;
            _entityQueryServiceFactory = entityQueryServiceFactory;
            _browserQueryService = browserQueryService;
            _blobStorage = blobStorage;
        }

        public HttpResponseMessage Post(CreateLikeModel like)
        {
            var qs = _entityQueryServiceFactory
                .GetQueryServiceForEntityTyp<QueryServiceInterface>
                (like.LikeEntity);

            if (qs == null)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            var entity = qs.FindById(like.EntityId) as EntityInterface;
            if (entity == null)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var likeCommand = new LikeCommand()
                                  {
                                      BrowserId = like.BrowserId,
                                      Comment = like.Comment.SafeText(),
                                      LikeEntity = entity,
                                      ILike = true//not doing not likes
                                  };

            var res = _commandBus.Send(likeCommand);
            return this.GetResponseForRes(res);
        }

        [Queryable]
        public IQueryable<LikeModel> Get(EntityTypeEnum entityTypeEnum, string id)
        {
            var qs = _entityQueryServiceFactory
                .GetQueryServiceForEntityTyp<QueryServiceInterface>(entityTypeEnum);
            return GetLikes(qs as QueryLikesInterface, id)
                .Select(like => like.FillBrowserModel(_browserQueryService, _blobStorage));
        }

        public static IQueryable<LikeModel> GetLikes(QueryLikesInterface queryLikes, string id)
        {
            if (queryLikes == null) return (new List<LikeModel>()).AsQueryable();
            return queryLikes.GetLikes(id)
                .Select(like => like.ToViewModel());
        }

        private IList<LikeModel> GetDummtDaya(string id)
        {
            var browserId = "c49c12fa-1024-4bd6-aad4-1fd5967de296";
            IList<LikeInterface> list = new List<LikeInterface>()
                                               {
                                                   new Like()
                                                       {
                                                           BrowserId = browserId,
                                                           EntityId = id,
                                                           LikeTime = DateTime.UtcNow,
                                                           ILike = true
                                                       },
                                                    new Like()
                                                    {
                                                            BrowserId = browserId,
                                                           EntityId = id,
                                                           LikeTime = DateTime.UtcNow,
                                                           ILike = true
                                                    },
                                                    new Like()
                                                       {
                                                           BrowserId = browserId,
                                                           EntityId = id,
                                                           LikeTime = DateTime.UtcNow,
                                                           ILike = true
                                                       },
                                                    new Like()
                                                    {
                                                           BrowserId = browserId,
                                                           EntityId = id,
                                                           LikeTime = DateTime.UtcNow,
                                                           ILike = true
                                                    }
                                               };
            return list.Select(c => c.ToViewModel(_browserQueryService, _blobStorage)).ToList();
        }
    }
}