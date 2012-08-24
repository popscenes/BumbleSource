using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Flier;
using WebSite.Application.Binding;
using PostaFlya.Binding;
using WebSite.Application.Content;
using WebSite.Common.Extension;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Likes;
using Website.Domain.Browser.Query;
using Website.Domain.Likes;
using Website.Domain.Likes.Command;
using Website.Domain.Content;

namespace PostaFlya.Controllers
{
    public class LikeController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _entityQueryService;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly GenericQueryServiceInterface _queryLikes;
        private readonly BlobStorageInterface _blobStorage;

        public LikeController(CommandBusInterface commandBus
            , GenericQueryServiceInterface entityQueryService
            , BrowserQueryServiceInterface browserQueryService
            , GenericQueryServiceInterface queryLikes
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _commandBus = commandBus;
            _entityQueryService = entityQueryService;
            _browserQueryService = browserQueryService;
            _queryLikes = queryLikes;
            _blobStorage = blobStorage;
        }

        public HttpResponseMessage Post(CreateLikeModel like)
        {
            var entity = _entityQueryService.FindById(GetTypeForLikeEntity(like.LikeEntity), like.EntityId) as EntityInterface;
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
            return GetLikes(_queryLikes, id)
                .Select(like => like.FillBrowserModel(_browserQueryService, _blobStorage));
        }

        public static IQueryable<LikeModel> GetLikes(GenericQueryServiceInterface queryLikes, string id)
        {
            if (queryLikes == null) return (new List<LikeModel>()).AsQueryable();
            return queryLikes.FindAggregateEntities<Like>(id)
                .Select(like => like.ToViewModel());
        }

        public static Type GetTypeForLikeEntity(EntityTypeEnum entityTypeEnum)
        {
            switch (entityTypeEnum)
            {
                case EntityTypeEnum.Flier:
                    return typeof (Flier);
                case EntityTypeEnum.Image:
                    return typeof (Image);
            }
            return typeof(Flier);
        }

        private IList<LikeModel> GetDummtDaya(string id)
        {
            var browserId = "c49c12fa-1024-4bd6-aad4-1fd5967de296";
            IList<LikeInterface> list = new List<LikeInterface>()
                                               {
                                                   new Like()
                                                       {
                                                           BrowserId = browserId,
                                                           AggregateId = id,
                                                           LikeTime = DateTime.UtcNow,
                                                           ILike = true
                                                       },
                                                    new Like()
                                                    {
                                                            BrowserId = browserId,
                                                           AggregateId = id,
                                                           LikeTime = DateTime.UtcNow,
                                                           ILike = true
                                                    },
                                                    new Like()
                                                       {
                                                           BrowserId = browserId,
                                                           AggregateId = id,
                                                           LikeTime = DateTime.UtcNow,
                                                           ILike = true
                                                       },
                                                    new Like()
                                                    {
                                                           BrowserId = browserId,
                                                           AggregateId = id,
                                                           LikeTime = DateTime.UtcNow,
                                                           ILike = true
                                                    }
                                               };
            return list.Select(c => c.ToViewModel(_browserQueryService, _blobStorage)).ToList();
        }
    }
}