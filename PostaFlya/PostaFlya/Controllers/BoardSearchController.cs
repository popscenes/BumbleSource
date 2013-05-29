using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Models;
using PostaFlya.Models.Board;
using PostaFlya.Models.Flier;

namespace PostaFlya.Controllers
{
    public class BoardSearchController : Controller
    {
        //
        // GET: /BoardSearch/

        public ActionResult Get(bool autosearch = false)
        {
            var model = new BoardSearchPageViewModel()
                {
                    PageId = autosearch ? WebConstants.BoardSearchPageAuto : WebConstants.BoardSearchPage
                };

            return View(model);
        }

    }
}
