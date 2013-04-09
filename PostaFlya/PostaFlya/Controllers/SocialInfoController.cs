using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website.Application.WebsiteInformation;

namespace PostaFlya.Controllers
{
    public class SocialController : Controller
    {
        private readonly WebsiteInfoServiceInterface _websiteInfoService;
        //
        // GET: /SocialInfo/

        public SocialController(WebsiteInfoServiceInterface websiteInfoService)
        {
            _websiteInfoService = websiteInfoService;
        }

        public ActionResult Info()
        {
            if (HttpContext.Request.Url != null)
            {
                var websiteInfo = _websiteInfoService.GetWebsiteInfo(HttpContext.Request.Url.Host);
                ViewBag.FBAppId = websiteInfo.FacebookAppID;
                ViewBag.FBPostRedirectUrl = Url.Action("PostRedirect", "Social", new {}, "http");
            }
            return View("_SocialInfoPartial");
        }

        public ActionResult PostRedirect()
        {
            return View();
        }
    }
}
