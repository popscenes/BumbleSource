using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ninject;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Flier.Analytic;
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
            ,int count = 40, int skip = 0, int distance = 0, string tags = "", string board="")
        {
            if(!loc.IsValid())
                return View(new List<BulletinFlierModel>());

            var model = BulletinApiController.GetFliers(_flierSearchService, _queryService, _blobStorage, _viewModelFactory
                             , loc, count, board: board, skip: skip, distance: distance, tags: tags);

            ViewBag.Location = loc;
            ViewBag.Distance = distance;
            ViewBag.Fliers = model;
            ViewBag.Tags = new Tags(tags);

            if (model.Count == count)
                ViewBag.NextPageUrl = Url.Action("Get", new {loc.Latitude, loc.Longitude, skip = skip + count, count, distance, tags});
            if(skip > 0)
                ViewBag.PrevPageUrl = Url.Action("Get", new { loc.Latitude, loc.Longitude, skip = Math.Max(skip - count, 0), count, distance, tags });

            return View("Get", model);
        }

        public ActionResult Detail(string id)
        {
            var model = BulletinApiController.GetDetail(id, _queryService, _behaviourQueryService, _blobStorage,
                                            _viewModelFactory, _browserInformation);


            ViewBag.FlierDetail = model;

            if(model != null)
                _webAnalyticService.RecordVisit(model.Flier.Id, FlierAnalyticSourceAction.IdByBulletin);
//            ViewBag.Comments = CommentController.GetComments(_queryService, id)
//                .Select(c => c.FillBrowserModel(_queryService, _blobStorage)).ToList();
//            ViewBag.Claims = ClaimController.GetClaims(_queryService, id).ToList();
//           
            return View("DetailGet", model);
        }
    }
}