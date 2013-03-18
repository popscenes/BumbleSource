using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PostaFlya.Application.Domain.Browser;
using Website.Application.Binding;
using PostaFlya.Models.Browser;
using Website.Application.Content;
using Website.Application.Domain.Browser;

namespace PostaFlya.Controllers
{
    public class BrowserInformationController : Controller
    {
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly BlobStorageInterface _blobStorage;

        public BrowserInformationController(PostaFlyaBrowserInformationInterface browserInformation
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _browserInformation = browserInformation;
            _blobStorage = blobStorage;
        }

        public ViewResult BrowserInfo()
        {
            //var js = new JsonResult {Data = _browserQueryService.ToCurrentBrowserModel()};
            var serializer = new JavaScriptSerializer();
            ViewBag.BrowserInfoJson = serializer.Serialize(_browserInformation.ToCurrentBrowserModel(_blobStorage));          
            return View("_BrowserInfoPartial");
        }
    }
}