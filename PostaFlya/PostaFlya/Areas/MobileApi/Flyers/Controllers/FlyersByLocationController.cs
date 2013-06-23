using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Areas.MobileApi.Infrastructure.Controller;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Controllers
{

    public class FlyersByLocationController : MobileApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;

        public FlyersByLocationController(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public ResponseContent<FlyerSummaryContent> Get([FromUri]FlyersByLocationRequest req)
        {
            var flyers = _queryChannel.Query(new FindFlyersByLocationAndDistanceQuery()
                {
                    Location = new Location(req.Long, req.Lat),
                    Distance = req.Distance,
                    Take = req.Take,
                    Skip = req.Skip
                }, new List<FlyerSummaryModel>());

            return ResponseContent<FlyerSummaryContent>.GetResponse(new FlyerSummaryContent() {Flyers = flyers});
        }
    }
}
