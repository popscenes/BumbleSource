using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Models.Board;
using PostaFlya.Models.Browser;
using Website.Application.Domain.Browser;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class BrowserInformationController : Controller
    {
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly QueryChannelInterface _queryChannel;

        public BrowserInformationController(PostaFlyaBrowserInformationInterface browserInformation, QueryChannelInterface queryChannel)
        {
            _browserInformation = browserInformation;
            _queryChannel = queryChannel;
        }

        public ViewResult BrowserInfo()
        {
            //var js = new JsonResult {Data = _browserQueryService.ToCurrentBrowserModel()};
            var serializer = new JavaScriptSerializer();
            var model = _queryChannel.ToViewModel<CurrentBrowserModel>(_browserInformation);
            //model.AdminBoards =

            if (!string.IsNullOrWhiteSpace(_browserInformation.Browser.EmailAddress))
            {
                var boards =
                    _queryChannel.Query(
                        new FindBoardByAdminEmailQuery() {AdminEmail = _browserInformation.Browser.EmailAddress},
                        (List<Board>) null);
                model.AdminBoards = _queryChannel.ToViewModel<BrowserInformationBoardModel, PostaFlya.Domain.Boards.Board>(boards);
            }

            ViewBag.BrowserInfoJson = serializer.Serialize(model);          
            return View("_BrowserInfoPartial");
        }
    }
}