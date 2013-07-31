using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website.Common.ApiInfrastructure.Controller;

namespace PostaFlya.Areas.WebApi.Flyers.Controllers
{
    public class WebFlyersByBrowserController : WebApiControllerBase
    {
        //
        // GET: /WebApi/WebFlyersByBrowser/

        public ActionResult Index()
        {
            return View();
        }

    }
}
