using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ninject;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Models;
using Website.Application.Binding;
using PostaFlya.Domain.Behaviour.Query;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Application.Content;
using Website.Application.Domain.Browser;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class BulletinController : Controller
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourQueryServiceInterface _behaviourQueryService;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;
        private readonly FlierSearchServiceInterface _flierSearchService;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly FlierWebAnalyticServiceInterface _webAnalyticService;

        public BulletinController(GenericQueryServiceInterface queryService
            , [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourQueryServiceInterface behaviourQueryService
            , FlierBehaviourViewModelFactoryInterface viewModelFactory
            , FlierSearchServiceInterface flierSearchService
            , PostaFlyaBrowserInformationInterface browserInformation
            , FlierWebAnalyticServiceInterface webAnalyticService)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
            _behaviourQueryService = behaviourQueryService;
            _viewModelFactory = viewModelFactory;
            _flierSearchService = flierSearchService;
            _browserInformation = browserInformation;
            _webAnalyticService = webAnalyticService;
        }

        public ActionResult Get(LocationModel loc
            ,int count = 40, string skipPast = "", int distance = 0, string tags = "", string board="")
        {

            var model = new BulletinBoardPageModel(){PageId = WebConstants.BulletinBoardPage};

            if (loc.IsValid())
                model.Fliers = BulletinApiController.GetFliers(_flierSearchService, _queryService, _blobStorage, _viewModelFactory
                             , loc, count, board: board, skipPast: skipPast, distance: distance, tags: tags);
      
            return View("Get", model);
        }

        public ActionResult Detail(string id)
        {
            var model = new BulletinDetailPageModel
                {
                    PageId = WebConstants.BulletinDetailPage,
                    Detail = BulletinApiController.GetDetail(id, _queryService, _behaviourQueryService, _blobStorage,
                                                              _viewModelFactory, _browserInformation)
                };

            if(model.Detail != null)
                _webAnalyticService.RecordVisit(model.Detail.Flier.Id, FlierAnalyticSourceAction.IdByBulletin);
         
            return View("DetailGet", model);
        }
    }
}