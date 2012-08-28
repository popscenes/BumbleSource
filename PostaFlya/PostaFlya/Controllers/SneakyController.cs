using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using Website.Application.Azure.Caching;

namespace PostaFlya.Controllers
{
    public class SneakyController : Controller
    {
        private readonly ObjectCache _cache;

        public SneakyController(ObjectCache cache)
        {
            _cache = cache;
        }

        //
        // GET: /Sneaky/

        public ActionResult ClearCache()
        {
            var cache = _cache as AzureCacheProvider;
            if(cache != null)
                cache.Clear();
            return RedirectToAction("Get", "Bulletin");
        }

    }
}
