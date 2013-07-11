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
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeHttp]
    public class MyImagesController : WebApiControllerBase
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly QueryChannelInterface _queryChannel;

        public MyImagesController(GenericQueryServiceInterface queryService,
            [ImageStorage]BlobStorageInterface blobStorage, QueryChannelInterface queryChannel)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
            _queryChannel = queryChannel;
        }

        public List<ImageViewModel> Get(string browserid)
        {
            return _queryChannel.Query(new GetByBrowserIdQuery<Image>() {BrowserId = browserid}, new List<Image>())
                                .Select(_ => _.ToViewModel().GetImageUrl(_blobStorage, false)).ToList();
        }
    }
}