using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Claims;
using PostaFlya.Binding;
using PostaFlya.Models.Flier;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Browser.Web;
using Website.Application.Domain.Obsolete;
using Website.Common.Extension;
using Website.Common.Model.Query;
using Website.Common.Obsolete;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using Website.Domain.Claims;
using Website.Domain.Claims.Command;

namespace PostaFlya.Controllers
{
    [Website.Application.Domain.Obsolete.BrowserAuthorizeHttp]
    public class ClaimController : OldWebApiControllerBase
    {
        private readonly MessageBusInterface _messageBus;
        private readonly UnitOfWorkForRepoFactoryInterface _uowFactory;
        private readonly QueryChannelInterface _queryChannel;


        public ClaimController(MessageBusInterface messageBus
            , UnitOfWorkForRepoFactoryInterface uowFactory
            , QueryChannelInterface queryChannel)
        {
            _messageBus = messageBus;
            _uowFactory = uowFactory;
            _queryChannel = queryChannel;
        }

        //note shouldn't be accessing query service from controller
        private GenericQueryServiceInterface QueryService
        {
            get { return _uowFactory.GetUowInContext().CurrentQuery; }
        }

        public HttpResponseMessage Post(CreateClaimModel claim)
        {
            var entity = QueryService.FindById(GetTypeForClaimEntity(claim.ClaimEntity), claim.EntityId) as EntityInterface;
            if (entity == null)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            EntityIdInterface ownerEntity = null;

            var browserIdInterface = entity as BrowserIdInterface;
            if (browserIdInterface != null)
            {
                var ownerId = browserIdInterface.BrowserId;
                ownerEntity = QueryService.FindById<Browser>(ownerId);
            }

            var claimCommand = new ClaimCommand()
                                  {
                                      BrowserId = claim.BrowserId,
                                      ClaimEntity = entity,
                                      Context = "tearoff",
                                      OwnerEntity = ownerEntity,
                                      Message = claim.ClaimerMessage 
                                  };

            _messageBus.Send(claimCommand);
            return this.GetResponseForRes(new MsgResponse() { IsError = false });            
        }

        // GET /api/Browser/browserId/claim/
        public IList<BulletinFlierSummaryModel> Get(string browserId)
        {
            var ret = _queryChannel.Query(new GetByBrowserIdQuery<Claim>() {BrowserId = browserId}, new List<Claim>())
                                   .Select(l => QueryService.FindById<Flier>(l.AggregateId))
                                   .Where(f => f.BrowserId != browserId);//exclude your own

            return _queryChannel.ToViewModel<BulletinFlierSummaryModel, Flier>(ret);
          
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