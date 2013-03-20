using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Claims;
using PostaFlya.Binding;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Browser.Web;
using Website.Common.Extension;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Domain.Claims;
using Website.Domain.Claims.Command;
using Website.Domain.Content;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeHttp]
    public class ClaimController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;



        public ClaimController(CommandBusInterface commandBus
            , QueryServiceForBrowserAggregateInterface queryService
            , [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            _commandBus = commandBus;
            _queryService = queryService;
            _blobStorage = blobStorage;
            _viewModelFactory = viewModelFactory;
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

            var claimCommand = new ClaimCommand()
                                  {
                                      BrowserId = claim.BrowserId,
                                      ClaimEntity = entity,
                                      Context = "tearoff",
                                      OwnerEntity = ownerEntity,
                                      Message = claim.ClaimerMessage 
                                  };

            var res = _commandBus.Send(claimCommand);
            return this.GetResponseForRes(res);
        }

        // GET /api/Browser/browserId/claim/
        public IList<BulletinFlierModel> Get(string browserId)
        {
            var ret = _queryService.GetByBrowserId<Claim>(browserId)
                      .Select(l => _queryService.FindById<Flier>(l.AggregateId))
                      .ToViewModel(_queryService, _blobStorage, _viewModelFactory);
            return ret;
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
    }
}