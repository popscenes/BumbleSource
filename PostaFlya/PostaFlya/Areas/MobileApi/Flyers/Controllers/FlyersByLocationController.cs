using System;
using System.Net;
using System.Web.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util;

namespace PostaFlya.Areas.MobileApi.Flyers.Controllers
{

    public class FlyersByLocationController : ApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly ConfigurationServiceInterface _config;

        public FlyersByLocationController(QueryChannelInterface queryChannel, ConfigurationServiceInterface config)
        {
            _queryChannel = queryChannel;
            _config = config;
        }

        public ResponseContent<FlyersByDateContent> Get([FromUri]FlyersByLocationRequest req)
        {
            var start = (req.Start != default(DateTimeOffset) ? req.Start : DateTimeOffset.UtcNow)
                .GetDateOnly();

            var suburb =
                _queryChannel.Query(
                    new PopulateSuburbQuery() { Suburb = new Suburb(){Longitude = req.Lng, Latitude = req.Lat} }
                    , (Suburb)null);

            if (suburb == null)
                this.ResponseError(HttpStatusCode.BadRequest, ResponseContent.StatusEnum.NotFound);

            var query = new FindFlyersByDateAndLocationQuery()
            {
                Location = suburb,
                Distance = req.Distance,
                Start = start,
                End = req.End != default(DateTimeOffset) ? req.End : start.AddDays(_config.GetSetting("DaySpan", 7))
            };
            var content = _queryChannel.Query(query, new FlyersByDateContent()
                , o => o.CacheFor(10.Minutes()));


            return ResponseContent<FlyersByDateContent>.GetResponse(content);
        }
    }
}
