using System.Web.Http;
using PostaFlya.Areas.WebApi.Location.Model;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.WebApi.Location.Controllers
{
    public class WebFindNearestSuburbController : WebApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;

        public WebFindNearestSuburbController(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public ResponseContent<SuburbModel> Get([FromUri] NearestSuburbByLocationRequest req)
        {
            var res = _queryChannel.Query(new FindNearestSuburbByGeoCoordsQuery()
                {
                    Geo = new GeoCoords(){Latitude = req.Lat, Longitude = req.Lng}

                }, default(SuburbModel));

            return ResponseContent<SuburbModel>.GetResponse(res);
        }
    }
}