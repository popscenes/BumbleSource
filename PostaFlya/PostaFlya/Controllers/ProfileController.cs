using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Domain.Flier;
using Website.Application.Binding;
using PostaFlya.Models.Factory;
using Website.Application.Content;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Browser.Web;
using Website.Domain.Browser.Query;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeMvc]
    public class ProfileController : Controller
    {
        private readonly BrowserInformationInterface _browserInformation;
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;

        public ProfileController( BrowserInformationInterface browserInformation
            , QueryServiceForBrowserAggregateInterface queryService, [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            _browserInformation = browserInformation;
            _queryService = queryService;
            _blobStorage = blobStorage;
            _viewModelFactory = viewModelFactory;
        }

        //
        // GET: /Profile/

        public ActionResult Get(string handle)
        {
            if(!_browserInformation.IsOrCanAccessBrowser(null, handle))
                return new HttpUnauthorizedResult();

            var model = ProfileApiController.GetForHandle(handle, _queryService,
                                                                     _blobStorage, _viewModelFactory);
            if (model == null)
                return new HttpNotFoundResult();

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
            var result = _queryService.FindFreeHandleForBrowser(handle, _browserInformation.Browser.Id);
            if (result == handle)
                return Json(true,  JsonRequestBehavior.AllowGet);

            string freeSuggestion = String.Format(CultureInfo.InvariantCulture,
                "{0} is not available. Suggested free handle {1}", handle, result);

            return Json(freeSuggestion, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PaymentPending()
        {
           
            return View();
        }
    }
}
