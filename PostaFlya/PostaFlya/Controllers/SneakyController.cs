using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Domain.Flier.Command;
using Website.Application.Azure.Caching;
using Website.Application.Domain.Browser.Web;
using Website.Domain.Browser.Command;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeMvc(Roles = "Admin")]
    public class SneakyController : Controller
    {
        private readonly ObjectCache _cache;
        private readonly MessageBusInterface _messageBus;

        public SneakyController(ObjectCache cache, MessageBusInterface messageBus)
        {
            _cache = cache;
            _messageBus = messageBus;
        }

        //
        // GET: /Sneaky/

        public ActionResult ClearCache()
        {
            var cache = _cache as AzureCacheProvider;
            if(cache != null)
                cache.Clear();
            return RedirectToAction("GigGuide", "Bulletin");
        }

        public ActionResult SetBrowserCredit(string browserId, double credit)
        {
            _messageBus.Send(new SetBrowserCreditCommand()
                {
                    BrowserId = browserId,
                    Credit = credit
                });
            return RedirectToAction("GigGuide", "Bulletin");
        }

        public ActionResult ReindexFlyers()
        {
//            _messageBus.Send(new ReindexFlyersCommand());
            return RedirectToAction("GigGuide", "Bulletin");
        }

    }
}
