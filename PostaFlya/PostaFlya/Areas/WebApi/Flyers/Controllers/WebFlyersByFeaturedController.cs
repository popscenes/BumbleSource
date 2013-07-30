using System.Web.Http;
using PostaFlya.Areas.WebApi.Flyers.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.WebApi.Flyers.Controllers
{
    public class WebFlyersByFeaturedController : WebApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;

        public WebFlyersByFeaturedController(QueryChannelInterface queryChannel)
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
