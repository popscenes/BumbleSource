﻿using System.Web.Http;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Domain.Flier;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Controllers
{
    public class FlyerDetailController : ApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;

        public FlyerDetailController(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public ResponseContent<FlyerDetailModel> Get([FromUri]FlyerDetailRequest req)
        {
            var flyer = _queryChannel.Query(new FindByIdQuery<Flier>()
                {
                    Id = req.Id.ToLower()
                }, (FlyerDetailModel)null);

            if (flyer == null)
                this.ResponseNotFound("Flyer {0} does not exist or is not active", req.Id);

            return ResponseContent<FlyerDetailModel>.GetResponse(flyer);
        }
    }
}
