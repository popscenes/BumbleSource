using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PostaFlya.Application.Domain.Browser;
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
            ViewBag.BrowserInfoJson = serializer.Serialize(model);          
            return View("_BrowserInfoPartial");
        }
    }
}