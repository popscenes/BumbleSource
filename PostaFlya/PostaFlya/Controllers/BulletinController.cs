using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebSite.Application.Binding;
using PostaFlya.Domain.Behaviour.Query;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using WebSite.Application.Content;
using Website.Domain.Browser.Query;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.Controllers
{
    public class BulletinController : Controller
    {
        private readonly FlierQueryServiceInterface _flierQueryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourQueryServiceInterface _behaviourQueryService;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;

        public BulletinController(FlierQueryServiceInterface flierQueryService
            , [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourQueryServiceInterface behaviourQueryService
            , FlierBehaviourViewModelFactoryInterface viewModelFactory
            , BrowserQueryServiceInterface browserQueryService)
        {
            _flierQueryService = flierQueryService;
            _blobStorage = blobStorage;
            _behaviourQueryService = behaviourQueryService;
            _viewModelFactory = viewModelFactory;
            _browserQueryService = browserQueryService;
        }

        public ActionResult Get(LocationModel loc
            ,int count = 40, int skip = 0, int distance = 0, string tags = "")
        {
            if(!loc.IsValid())
                return View(new List<BulletinFlierModel>());

            var model =  BulletinApiController.GetFliers(_flierQueryService, _blobStorage, _viewModelFactory
                             , loc, count, skip, distance, tags);

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
            var model = BulletinApiController.GetDetail(id, _flierQueryService, _behaviourQueryService, _blobStorage,
                                            _viewModelFactory);

            ViewBag.FlierDetail = model;
            ViewBag.Comments = CommentController.GetComments(_flierQueryService, id)
                .Select(c => c.FillBrowserModel(_browserQueryService, _blobStorage)).ToList();
            ViewBag.Likes = LikeController.GetLikes(_flierQueryService, id)
                .Select(l => l.FillBrowserModel(_browserQueryService, _blobStorage)).ToList();
           
            return View("DetailGet", model);
        }
    }
}