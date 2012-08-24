using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSite.Application.Binding;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Factory;
using WebSite.Application.Content;
using Website.Application.Domain.Browser;
using Website.Domain.Browser.Query;

namespace PostaFlya.Controllers
{
    public class ProfileController : Controller
    {
        private readonly BrowserInformationInterface _browserInformation;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly FlierQueryServiceInterface _flierQueryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;

        public ProfileController( BrowserInformationInterface browserInformation
            , BrowserQueryServiceInterface browserQueryService
            , FlierQueryServiceInterface flierQueryService
            , [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            _browserInformation = browserInformation;
            _browserQueryService = browserQueryService;
            _flierQueryService = flierQueryService;
            _blobStorage = blobStorage;
            _viewModelFactory = viewModelFactory;
        }

        //
        // GET: /Profile/

        public ActionResult Get(string name)
        {
            var model = ProfileApiController.GetForHandle(name, _browserQueryService, _flierQueryService,
                                                                     _blobStorage, _viewModelFactory);
            ViewBag.ProfileModel = model;
            return View(model);
        }

        [ActionName("View")]
        public ActionResult OwnView()
        {
            return View("Get");
        }

        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult CheckHandle(string handle)
        {
            var result = _browserQueryService.FindFreeHandle(handle, _browserInformation.Browser.Id);
            if (result == handle)
                return Json(true,  JsonRequestBehavior.AllowGet);

            string freeSuggestion = String.Format(CultureInfo.InvariantCulture,
                "{0} is not available. Suggested free handle {1}", handle, result);

            return Json(freeSuggestion, JsonRequestBehavior.AllowGet);
        }

    }
}
