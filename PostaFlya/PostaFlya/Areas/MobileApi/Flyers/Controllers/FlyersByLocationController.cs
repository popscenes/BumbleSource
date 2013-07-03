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

        public ResponseContent<FlyersByDateContent> Get([FromUri]FlyersByLocationRequest req)
        {
            var content = _queryChannel.Query(new FindFlyersByDateAndLocationQuery()
                {
                    Location = new Location(req.Lng, req.Lat),
                    Distance = req.Distance,
                    Start = req.Start != default(DateTimeOffset) ? req.Start : DateTimeOffset.UtcNow,
                    End = req.End != default(DateTimeOffset) ? req.End : DateTimeOffset.UtcNow.AddDays(4)
                }, new FlyersByDateContent());

            return ResponseContent<FlyersByDateContent>.GetResponse(content);
        }
    }
}
