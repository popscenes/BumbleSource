using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PostaFlya.Controllers
{
    public class StaticController : Controller
    {
        //
        // GET: /Static/

        public ActionResult TermsOfService()
        {
            return View();
        }

        public ActionResult PrivacyPolicy()
        {
            return View();
        }

    }
}
