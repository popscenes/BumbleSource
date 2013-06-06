using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Website.Application.Binding;
using PostaFlya.Models.Content;
using Website.Application.Content;
using Website.Application.Domain.Browser.Web;
using Website.Common.Controller;
using Website.Domain.Browser.Query;
using Website.Domain.Content;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeHttp]
    public class MyImagesController : WebApiControllerBase
    {
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;

        public MyImagesController(QueryServiceForBrowserAggregateInterface queryService,
            [ImageStorage]BlobStorageInterface blobStorage)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
        }

        public List<ImageViewModel> Get(string browserid)
        {
            return _queryService.GetByBrowserId<Image>(browserid).Select(_ => _.ToViewModel().GetImageUrl(_blobStorage, false)).ToList();
        }
    }
}