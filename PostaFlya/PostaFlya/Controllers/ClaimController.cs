using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Claims;
using WebSite.Application.Binding;
using PostaFlya.Binding;
using WebSite.Application.Content;
using WebSite.Common.Extension;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using PostaFlya.Models.Browser;
using Website.Domain.Browser.Query;
using Website.Domain.Claims;
using Website.Domain.Claims.Command;
using Website.Domain.Content;

namespace PostaFlya.Controllers
{
    public class ClaimController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _entityQueryService;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly GenericQueryServiceInterface _queryClaims;
        private readonly BlobStorageInterface _blobStorage;

        public ClaimController(CommandBusInterface commandBus
            , GenericQueryServiceInterface entityQueryService
            , BrowserQueryServiceInterface browserQueryService
            , GenericQueryServiceInterface queryClaims
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _commandBus = commandBus;
            _entityQueryService = entityQueryService;
            _browserQueryService = browserQueryService;
            _queryClaims = queryClaims;
            _blobStorage = blobStorage;
        }

        public HttpResponseMessage Post(CreateClaimModel claim)
        {
            var entity = _entityQueryService.FindById(GetTypeForClaimEntity(claim.ClaimEntity), claim.EntityId) as EntityInterface;
            if (entity == null)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var claimCommand = new ClaimCommand()
                                  {
                                      BrowserId = claim.BrowserId,
                                      ClaimEntity = entity,
                                  };

            var res = _commandBus.Send(claimCommand);
            return this.GetResponseForRes(res);
        }

        [Queryable]
        public IQueryable<ClaimModel> Get(EntityTypeEnum entityTypeEnum, string id)
        {
            return GetClaims(_queryClaims, id)
                .Select(claim => claim.FillBrowserModel(_browserQueryService, _blobStorage));
        }

        public static IQueryable<ClaimModel> GetClaims(GenericQueryServiceInterface queryClaims, string id)
        {
            if (queryClaims == null) return (new List<ClaimModel>()).AsQueryable();
            return queryClaims.FindAggregateEntities<Claim>(id)
                .Select(claim => claim.ToViewModel());
        }

        public static Type GetTypeForClaimEntity(EntityTypeEnum entityTypeEnum)
        {
            switch (entityTypeEnum)
            {
                case EntityTypeEnum.Flier:
                    return typeof (Flier);
//                case EntityTypeEnum.Image:
//                    return typeof (Image);
            }
            return typeof(Flier);
        }

        private IList<ClaimModel> GetDummtDaya(string id)
        {
            var browserId = "c49c12fa-1024-4bd6-aad4-1fd5967de296";
            IList<ClaimInterface> list = new List<ClaimInterface>()
                                               {
                                                   new Claim()
                                                       {
                                                           BrowserId = browserId,
                                                           AggregateId = id,
                                                           ClaimTime = DateTime.UtcNow,
                                                       },
                                                    new Claim()
                                                    {
                                                            BrowserId = browserId,
                                                           AggregateId = id,
                                                           ClaimTime = DateTime.UtcNow,
                                                    },
                                                    new Claim()
                                                       {
                                                           BrowserId = browserId,
                                                           AggregateId = id,
                                                           ClaimTime = DateTime.UtcNow,
                                                       },
                                                    new Claim()
                                                    {
                                                           BrowserId = browserId,
                                                           AggregateId = id,
                                                           ClaimTime = DateTime.UtcNow,
                                                    }
                                               };
            return list.Select(c => c.ToViewModel(_browserQueryService, _blobStorage)).ToList();
        }
    }
}