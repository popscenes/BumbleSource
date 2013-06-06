using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Flier;
using PostaFlya.Models;
using PostaFlya.Models.Browser;
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
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;

        public ProfileController(PostaFlyaBrowserInformationInterface browserInformation
            , QueryServiceForBrowserAggregateInterface queryService, [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            _browserInformation = browserInformation;
            _queryService = queryService;
            _blobStorage = blobStorage;
            _viewModelFactory = viewModelFactory;
        }

        public ActionResult Posted()
        {
            return View(new ProfileEditModel { PageId = WebConstants.ProfilePostedPage });            
        }

        public ActionResult PaymentPending()
        {
            return View(new ProfileEditModel { PageId = WebConstants.ProfilePaymentPage });            
        }

        public ActionResult Peeled()
        {
            return View(new ProfileEditModel { PageId = WebConstants.ProfilePeeledPage });
        }

        public ActionResult Edit()
        {
            return View(new ProfileEditModel{ PageId = WebConstants.ProfileEditPage});
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


    }
}
