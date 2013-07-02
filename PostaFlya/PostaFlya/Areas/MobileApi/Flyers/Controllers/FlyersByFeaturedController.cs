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
    public class FlyersByFeaturedController : MobileApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;

        public FlyersByFeaturedController(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public ResponseContent<FlyersByFeaturedContent> Get([FromUri]FlyersByLatestRequest req)
        {
            var content = _queryChannel.Query(new FindFlyersByFeaturedQuery()
                {
                    Take = req.Take,
                    Skip = req.Skip
                }, new FlyersByFeaturedContent());

            return ResponseContent<FlyersByFeaturedContent>.GetResponse(content);
        }
    }
}
