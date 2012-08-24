using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebSite.Application.Binding;
using PostaFlya.Models.Browser;
using WebSite.Application.Content;
using Website.Application.Domain.Browser;

namespace PostaFlya.Controllers
{
    public class BrowserInformationController : Controller
    {
        private readonly BrowserInformationInterface _browserQueryService;
        private readonly BlobStorageInterface _blobStorage;

        public BrowserInformationController(BrowserInformationInterface browserQueryService
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _browserQueryService = browserQueryService;
            _blobStorage = blobStorage;
        }

        public ViewResult BrowserInfo()
        {
            //var js = new JsonResult {Data = _browserQueryService.ToCurrentBrowserModel()};
            var serializer = new JavaScriptSerializer();
            
            ViewBag.BrowserInfoJson = serializer.Serialize(_browserQueryService.ToCurrentBrowserModel(_blobStorage));          
            return View("_BrowserInfoPartial");
        }
    }
}