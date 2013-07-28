using System.Web.Mvc;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Models;
using Website.Application.Binding;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Application.Content;
using Website.Application.Domain.Location.Query;
using Website.Application.WebsiteInformation;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Controllers
{
    public class BulletinController : Controller
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierSearchServiceInterface _flierSearchService;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly FlierWebAnalyticServiceInterface _webAnalyticService;
        private readonly WebsiteInfoServiceInterface _websiteInfoService;
        private readonly QueryChannelInterface _queryChannel;

        public BulletinController(GenericQueryServiceInterface queryService
            , [ImageStorage]BlobStorageInterface blobStorage
            , FlierSearchServiceInterface flierSearchService
            , PostaFlyaBrowserInformationInterface browserInformation
            , FlierWebAnalyticServiceInterface webAnalyticService
            ,WebsiteInfoServiceInterface websiteInfoService, QueryChannelInterface queryChannel)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
            _flierSearchService = flierSearchService;
            _browserInformation = browserInformation;
            _webAnalyticService = webAnalyticService;
            _websiteInfoService = websiteInfoService;
            _queryChannel = queryChannel;
        }

        public ActionResult Get(LocationModel loc
            ,int count = 40, string skipPast = "", int distance = 0, string tags = "", string board="")
        {

            var model = new BulletinBoardPageModel(){PageId = WebConstants.BulletinBoardPage};

            if (loc.IsValid())
                model.Fliers = BulletinApiController.GetFliers(_flierSearchService, _queryChannel, _queryService 
                             , loc, count, board: board, skipPast: skipPast, distance: distance, tags: tags);
      
            return View("Get", model);
        }
       
        public ActionResult GigGuide()
        {
            var model = new BulletinBoardPageModel() { PageId = WebConstants.GigGuidePage };

            if (_browserInformation.LastSearchLocation == null || !_browserInformation.LastSearchLocation.IsValid)
            {
                _browserInformation.LastSearchLocation = _queryChannel.Query(new GetLocationFromIdQuery()
                    {
                        IpAddress = _browserInformation.IpAddress
                    }, new Location());
            }

            return View("GigGuide", model); 
        }

        public ActionResult Detail(string id)
        {
            var model = new BulletinDetailPageModel
                {
                    PageId = WebConstants.BulletinDetailPage,
                    Detail = BulletinApiController.GetDetail(id, _queryChannel)
                };


            if (model.Detail == null || 
                (model.Detail.Flier.Status.AsEnum<FlierStatus>() != FlierStatus.Active
                && !_browserInformation.Browser.IsOwner(model.Detail.Flier)))
            {
                return new HttpNotFoundResult();
            }

            _webAnalyticService.RecordVisit(model.Detail.Flier.Id, FlierAnalyticSourceAction.IdByBulletin);
            return View("DetailGet", model);
        }
    }
}