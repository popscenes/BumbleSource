using System;
using System.Globalization;
using System.Web.Mvc;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Models;
using PostaFlya.Models.Browser;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Browser.Web;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeMvc]
    public class ProfileController : Controller
    {
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly QueryChannelInterface _queryChannel;

        public ProfileController(PostaFlyaBrowserInformationInterface browserInformation
            , QueryChannelInterface queryChannel)
        {
            _browserInformation = browserInformation;
            _queryChannel = queryChannel;
        }

        public ActionResult Posted()
        {
            return View(new ProfileEditModel { PageId = WebConstants.ProfilePostedPage });            
        }

        public ActionResult Boards()
        {
            return View(new ProfileEditModel { PageId = WebConstants.ProfileBoardsPage });
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
            var result =
                _queryChannel.FindFreeHandleForBrowser(handle, _browserInformation.Browser.Id);
            if (result == handle)
                return Json(true,  JsonRequestBehavior.AllowGet);

            string freeSuggestion = String.Format(CultureInfo.InvariantCulture,
                "{0} is not available. Suggested free handle {1}", handle, result);

            return Json(freeSuggestion, JsonRequestBehavior.AllowGet);
        }


    }
}
