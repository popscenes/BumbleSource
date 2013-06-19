using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Controllers
{
    public class FlyersByLatestController : ApiController
    {
        private readonly QueryChannelInterface _queryChannel;

        public FlyersByLatestController(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public ResponseContent<FlyerSummaryContent> Get([FromUri]FlyersByLatestRequest req)
        {
            var flyers = _queryChannel.Query(new FindFlyersByLatestQuery()
                {
                    Take = req.Take
                }, new List<FlyerSummaryModel>());

            return ResponseContent<FlyerSummaryContent>.GetResponse(new FlyerSummaryContent() {Flyers = flyers});
        }
    }
}
