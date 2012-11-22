using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Claims;
using Website.Application.Binding;
using PostaFlya.Binding;
using Website.Application.Content;
using Website.Common.Extension;
using Website.Domain.Browser;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Domain.Claims;
using Website.Domain.Claims.Command;
using Website.Domain.Content;

namespace PostaFlya.Controllers
{
    public class ClaimController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;

        public ClaimController(CommandBusInterface commandBus
            , GenericQueryServiceInterface queryService
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _commandBus = commandBus;
            _queryService = queryService;
            _blobStorage = blobStorage;
        }

        public HttpResponseMessage Post(CreateClaimModel claim)
        {
            var entity = _queryService.FindById(GetTypeForClaimEntity(claim.ClaimEntity), claim.EntityId) as EntityInterface;
            if (entity == null)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            EntityIdInterface ownerEntity = null;

            var browserIdInterface = entity as BrowserIdInterface;
            if (browserIdInterface != null)
            {
                var ownerId = browserIdInterface.BrowserId;
                ownerEntity = _queryService.FindById<Browser>(ownerId);
            }

            String context = "tearoff";

            if (claim.SendClaimerContactDetails)
            {
                context = "senduserdetails";
            }

            var claimCommand = new ClaimCommand()
                                  {
                                      BrowserId = claim.BrowserId,
                                      ClaimEntity = entity,
                                      Context = context,
                                      OwnerEntity = ownerEntity,
                                      Message = claim.ClaimerMessage 
                                  };

            var res = _commandBus.Send(claimCommand);
            return this.GetResponseForRes(res);
        }

        public IQueryable<ClaimModel> Get(EntityTypeEnum entityTypeEnum, string id)
        {
            return GetClaims(_queryService, id)
                .Select(claim => claim.FillBrowserModel(_queryService, _blobStorage));
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
            return list.Select(c => c.ToViewModel(_queryService, _blobStorage)).ToList();
        }
    }
}