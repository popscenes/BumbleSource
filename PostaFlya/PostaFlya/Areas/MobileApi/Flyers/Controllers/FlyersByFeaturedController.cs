using System.Web.Http;
using System.Web.Http.Description;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class FlyersByFeaturedController : ApiControllerBase
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
