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
        

        public TrackViewController(PostaFlyaBrowserInformationInterface browserInformation
            , ConfigurationServiceInterface configurationService
            , GenericQueryServiceInterface queryService
            , FlierWebAnalyticServiceInterface webAnalyticService)
        {
            _browserInformation = browserInformation;
            _configurationService = configurationService;
            _queryService = queryService;
            _webAnalyticService = webAnalyticService;
        }

        public ActionResult Index(string t, string id)
        {
            _browserInformation.TrackingId = t;

            var siteUrl = _configurationService.GetSetting("SiteUrl") ?? "";
            return RedirectPermanent(siteUrl + "/" + id);
        }

        public ActionResult Loc([FromUri]LocationModel loc, string id)
        {
            var flier = _queryService.FindByFriendlyId<Flier>(id.Trim('/'));
            if(flier == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            _webAnalyticService.RecordVisit(flier.Id, FlierAnalyticSourceAction.LocationTrack, loc.ToDomainModel());

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

    }
}
