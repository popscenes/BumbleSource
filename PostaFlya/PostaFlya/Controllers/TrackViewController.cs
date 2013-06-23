using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Models.Location;
using Website.Domain.Location;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class TrackViewController : Controller
    {
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly FlierWebAnalyticServiceInterface _webAnalyticService;
        private readonly QueryChannelInterface _queryChannel;
        

        public TrackViewController(PostaFlyaBrowserInformationInterface browserInformation
            , ConfigurationServiceInterface configurationService
            , GenericQueryServiceInterface queryService
            , FlierWebAnalyticServiceInterface webAnalyticService, QueryChannelInterface queryChannel)
        {
            _browserInformation = browserInformation;
            _configurationService = configurationService;
            _queryService = queryService;
            _webAnalyticService = webAnalyticService;
            _queryChannel = queryChannel;
        }

        public ActionResult Index(string src, string id)
        {
            if (string.IsNullOrWhiteSpace(_browserInformation.TrackingId))
                _browserInformation.TrackingId = Guid.NewGuid().ToString();
            
            _browserInformation.LastSearchLocation = null;

            _webAnalyticService.RecordVisit(id, FlierAnalyticUrlUtil.GetSourceAction(src, FlierAnalyticSourceAction.TinyUrl));

            var siteUrl = _configurationService.GetSetting("SiteUrl") ?? "";
            return RedirectPermanent(siteUrl + "/" + id);
        }

        public ActionResult Loc([FromUri]LocationModel loc, string id)
        {
            var flier = _queryChannel.Query(new FindByFriendlyIdQuery() { FriendlyId = id.Trim('/') }, (Flier)null);

            if(flier == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            _webAnalyticService.RecordVisit(flier.Id, FlierAnalyticSourceAction.LocationTrack, loc.ToDomainModel());

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

    }
}
